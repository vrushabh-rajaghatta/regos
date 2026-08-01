using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// A tenant's product in <em>one</em> regulatory jurisdiction — the tier
/// between the global identity and the licences granted over it.
/// </summary>
/// <remarks>
/// <b>It is identified by its own identity, not by the (global product,
/// country) pair.</b> Several medicinal products may exist for the same pair
/// when the business distinguishes them — different presentations, strengths,
/// or the two halves of a partial divestment — so nothing enforces uniqueness
/// on it, and nothing resolves-or-creates one on a caller's behalf.
/// <para>
/// It exists because a market presence and a licence are different facts. A
/// medicinal product means <em>"we market, or intend to market, this product
/// here"</em>, and can hold that meaning for years with zero registrations:
/// dossier preparation, labelling, artwork, pricing and launch planning all
/// precede authorisation. The dependency runs <c>GlobalProduct →
/// MedicinalProduct → Registration</c>, and only ever that way.
/// </para>
/// <para>
/// Deliberately almost empty. Trade name (S002), market status (S003), ATC code
/// and strength (EPIC-010) and the local label (EPIC-018) all belong here, and
/// each arrives with the feature that reads it. What this story establishes is
/// the identity they hang from.
/// </para>
/// <para>
/// Lives beside <see cref="GlobalProduct"/> rather than in a folder of its own:
/// the folder is named for the context's domain, not for one aggregate, and
/// keeping both tiers in it avoids the namespace-equals-type-name collision
/// that S000 was able to delete fourteen <c>using</c> aliases to remove.
/// </para>
/// </remarks>
public sealed class MedicinalProduct : AggregateRoot<MedicinalProductId>
{
    private readonly List<TradeName> _tradeNames = [];
    private readonly List<MarketStatusEntry> _marketStatusHistory = [];

    // Parameterized private constructor, no parameterless one: EF binds by
    // parameter name, and this keeps every field non-nullable. Same shape as
    // GlobalProduct beside it.
    private MedicinalProduct(
        MedicinalProductId id,
        TenantId tenantId,
        GlobalProductId globalProductId,
        CountryId countryId,
        MedicinalProductStatus status,
        DateOnly statusDate)
    {
        Id = id;
        TenantId = tenantId;
        GlobalProductId = globalProductId;
        CountryId = countryId;
        Status = status;
        StatusDate = statusDate;
    }

    /// <summary>
    /// The owning tenant (ADR-031). Set once: moving a market-local record
    /// between tenants would be a transfer, with its own rules.
    /// </summary>
    public TenantId TenantId { get; }

    /// <summary>
    /// The global identity this localises. Immutable — repointing it would
    /// silently rewrite what every licence beneath it authorises.
    /// </summary>
    public GlobalProductId GlobalProductId { get; }

    /// <summary>
    /// The jurisdiction. Immutable for the same reason, and the single
    /// authoritative answer to "which market is this registration for?" —
    /// <c>Registration</c> deliberately does not carry a second copy.
    /// </summary>
    public CountryId CountryId { get; }

    /// <summary>
    /// Whether this record participates in normal work — an activation flag,
    /// not the market status. See <see cref="MedicinalProductStatus"/>.
    /// </summary>
    public MedicinalProductStatus Status { get; private set; }

    public DateOnly StatusDate { get; private set; }

    /// <summary>
    /// What the product is called here — one name per language. Empty is
    /// ordinary: a market presence exists before the branding is settled.
    /// </summary>
    public IReadOnlyCollection<TradeName> TradeNames => _tradeNames.AsReadOnly();

    /// <summary>
    /// What is commercially true here — <b>not</b> <see cref="Status"/>, which
    /// says whether this record is in use. A discontinued product's record is
    /// still perfectly active; a deactivated record may describe a market that
    /// is still selling. The two never merge.
    /// </summary>
    /// <remarks>
    /// Stored, not replayed: the portfolio views read one indexed column rather
    /// than reducing a history per row. <see cref="MarketStatusHistory"/>
    /// records how it reached this.
    /// </remarks>
    public MarketStatus CurrentMarketStatus { get; private set; }

    /// <summary>
    /// Every market status this presence has held, oldest first. Append-only.
    /// </summary>
    public IReadOnlyCollection<MarketStatusEntry> MarketStatusHistory
        => _marketStatusHistory.AsReadOnly();

    /// <param name="statusDate">
    /// The business date this market-local record came into being — supplied,
    /// never read from the clock, so a migrated portfolio can state when the
    /// market presence actually began.
    /// </param>
    public static MedicinalProduct Create(
        TenantId tenantId,
        GlobalProductId globalProductId,
        CountryId countryId,
        DateOnly statusDate)
    {
        if (tenantId is null)
            throw new DomainException(MedicinalProductErrors.TenantRequired);

        if (globalProductId is null)
            throw new DomainException(
                MedicinalProductErrors.GlobalProductRequired);

        if (countryId == default)
            throw new DomainException(MedicinalProductErrors.CountryRequired);

        if (statusDate == default)
            throw new DomainException(
                MedicinalProductErrors.StatusDateRequired);

        var medicinalProduct = new MedicinalProduct(
            MedicinalProductId.New(),
            tenantId,
            globalProductId,
            countryId,
            MedicinalProductStatus.Active,
            statusDate);

        // The first history entry is the status it starts in, not a separate
        // "created" event: the history is one chronological sequence of the
        // states held, in one vocabulary. The same shape Registration uses.
        //
        // One supplied date, two derived facts — the record became active and
        // the plan began on the same real-world day, and that is honest rather
        // than a conflation: nothing later moves them together.
        medicinalProduct.Record(MarketStatus.Planned, statusDate, null);

        return medicinalProduct;
    }

    /// <summary>
    /// Retires this record from normal work. Nothing beneath it changes.
    /// </summary>
    /// <remarks>
    /// <b>Nothing here consults the registrations held in this market</b>, and
    /// that is deliberate twice over.
    /// <para>
    /// First, it would reverse the dependency this tier exists to establish:
    /// <c>GlobalProduct → MedicinalProduct → Registration</c> runs one way, and
    /// a parent inspecting its dependants across an aggregate boundary is the
    /// first crack in it.
    /// </para>
    /// <para>
    /// Second — and the stronger reason — <em>"has registrations"</em> is not
    /// the invariant anyone actually means. An expired one should not block
    /// this; nor a withdrawn one; nor a superseded one. The rule immediately
    /// becomes <em>"has registrations whose current status is…"</em>, which is
    /// a policy over another aggregate's lifecycle. The more a rule depends on
    /// <c>Registration</c> semantics, the more clearly it belongs with
    /// <c>Registration</c>. If it is ever required, it arrives as an
    /// application-level policy that reads registrations and then calls this —
    /// and the aggregate stays ignorant.
    /// </para>
    /// <para>
    /// "This market has five registrations, are you sure?" is a <b>warning</b>,
    /// and warnings help humans. Domain rules preserve truth, and nothing about
    /// this makes a truth impossible to represent.
    /// </para>
    /// </remarks>
    public void Deactivate(DateOnly on)
    {
        if (Status == MedicinalProductStatus.Inactive)
            throw new BusinessRuleViolationException(
                MedicinalProductErrors.AlreadyInactive);

        Status = MedicinalProductStatus.Inactive;
        StatusDate = Dated(on);
    }

    /// <remarks>
    /// Not idempotent, deliberately, and for the same reason
    /// <c>OrganizationDivision</c> is not: a caller asking for a state the
    /// record already holds is acting on a stale view, and silently succeeding
    /// would hide that.
    /// </remarks>
    public void Activate(DateOnly on)
    {
        if (Status == MedicinalProductStatus.Active)
            throw new BusinessRuleViolationException(
                MedicinalProductErrors.AlreadyActive);

        Status = MedicinalProductStatus.Active;
        StatusDate = Dated(on);
    }

    private static DateOnly Dated(DateOnly on)
        => on == default
            ? throw new DomainException(
                MedicinalProductErrors.StatusDateRequired)
            : on;

    /// <summary>
    /// Records what became commercially true here, and when.
    /// </summary>
    /// <remarks>
    /// <b>There is no transition table.</b> <c>RegistrationLifecycle</c> exists
    /// because a regulator's decision graph is genuinely constrained; commercial
    /// reality is not. A product may be launched, become unavailable, return,
    /// and be discontinued and relaunched years later, and none of that is
    /// incoherent. Encoding one company's history as universal law is exactly
    /// what that lifecycle's own governing principle forbids.
    /// <para>
    /// Two rules survive, because they are about coherence rather than process:
    /// a status cannot be re-entered from itself, and business time moves
    /// forward. A third is specific to <see cref="MarketStatus.Planned"/> — a
    /// market already entered cannot be planned again.
    /// </para>
    /// </remarks>
    /// <param name="occurredOn">
    /// The business date this became true — never the clock, and never earlier
    /// than the status it replaces.
    /// </param>
    public void ChangeMarketStatus(
        MarketStatus target,
        DateOnly occurredOn,
        string? note = null)
    {
        if (!Enum.IsDefined(target))
            throw new DomainException(
                MedicinalProductErrors.MarketStatusNotRecognised);

        if (occurredOn == default)
            throw new DomainException(
                MedicinalProductErrors.OccurredOnRequired);

        // The one genuinely incoherent transition. Planned means "we intend to
        // be here"; a market that has been entered cannot be intended again.
        // This is why the initial state is Planned rather than NotLaunched —
        // the latter reads as a reversible observation and would need this rule
        // explained in prose instead of enforced.
        if (target == MarketStatus.Planned)
            throw new BusinessRuleViolationException(
                MedicinalProductErrors.MarketCannotBePlannedAgain);

        if (target == CurrentMarketStatus)
            throw new BusinessRuleViolationException(
                MedicinalProductErrors.AlreadyInMarketStatus(target));

        // Two entries may share a date — a migration routinely produces that —
        // but a status taking effect before the one it replaces would make the
        // history unreadable. Discovering an earlier event later is a
        // correction, which is a separate concept.
        if (occurredOn < _marketStatusHistory[^1].OccurredOn)
            throw new DomainException(
                MedicinalProductErrors.OccurredOnBeforePreviousEntry);

        Record(target, occurredOn, note);
    }

    /// <summary>
    /// The core invariant: <b>every change updates
    /// <see cref="CurrentMarketStatus"/> and appends exactly one immutable
    /// entry.</b> Nothing else writes either, so current state and the record
    /// of how it was reached can never disagree.
    /// </summary>
    private void Record(MarketStatus status, DateOnly occurredOn, string? note)
    {
        if (note is not null
            && note.Trim().Length > MarketStatusEntry.NoteMaxLength)
        {
            throw new DomainException(MedicinalProductErrors.NoteTooLong);
        }

        CurrentMarketStatus = status;

        _marketStatusHistory.Add(new MarketStatusEntry(
            MarketStatusEntryId.New(),
            status,
            occurredOn,
            DateTime.UtcNow,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim()));
    }

    /// <summary>
    /// Records what this product is called here, in one language.
    /// </summary>
    /// <remarks>
    /// One per language, and this is the <em>deliberate opposite</em> of the
    /// rule one tier up. Two market presences in one country are two business
    /// objects a company may legitimately hold; two English names for one
    /// market presence are two labels for one thing, so one of them is wrong.
    /// Different concepts, different invariants.
    /// <para>
    /// The database carries the same rule as a unique index, so a race between
    /// two concurrent requests cannot slip a second one past this check.
    /// </para>
    /// </remarks>
    public TradeName AddTradeName(LanguageCode language, string? name)
    {
        if (language is null)
            throw new DomainException(MedicinalProductErrors.LanguageRequired);

        if (_tradeNames.Any(x => x.Language == language))
            throw new BusinessRuleViolationException(
                MedicinalProductErrors.TradeNameLanguageAlreadyRecorded);

        var tradeName = new TradeName(TradeNameId.New(), language, name);

        _tradeNames.Add(tradeName);

        return tradeName;
    }

    /// <remarks>
    /// There is no Rename, deliberately. Without effective dating a rename is
    /// indistinguishable from remove-then-add, and offering one would imply a
    /// historical identity the model does not keep. When regulators care that
    /// <em>Brand A became Brand B</em>, that arrives as dating or status
    /// history, and renaming becomes a distinct act worth naming.
    /// </remarks>
    public void RemoveTradeName(TradeNameId tradeNameId)
    {
        var tradeName = _tradeNames.FirstOrDefault(x => x.Id == tradeNameId)
            ?? throw new NotFoundException(
                MedicinalProductErrors.TradeNameNotFound);

        _tradeNames.Remove(tradeName);
    }
}
