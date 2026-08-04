using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Domain.Product;

/// <summary>
/// What a market actually sells — a carton of thirty tablets, a single 5 mL
/// vial, a wallet of fourteen.
/// </summary>
/// <remarks>
/// <b>A pack is how a medicine is supplied, not what it is</b>
/// (<see href="../../../docs/adr/ADR-061-a-pack-is-how-a-medicine-is-supplied.md">ADR-061</see>).
/// The discriminator that keeps this out of
/// <see cref="MedicinalProductComponent"/>: <em>does it change when the same
/// medicine is sold in a different pack size?</em> A 30-tablet carton and a
/// 100-tablet carton share an identical component tree — one tablet, one dose
/// form, one composition — and differ entirely here. Modelling packs as
/// components would duplicate that tree once per pack size.
/// <para>
/// Stated as a pair: <b>a component has a dose form; a pack has a size.</b> The
/// material each layer is made of arrives with <c>PackageItem</c> in S002.
/// </para>
/// <para>
/// <b>Market-local, like everything else in this tier.</b> France's 28s and the
/// UK's 30s are different packs, not one pack with a country column
/// (<see href="../../../docs/adr/ADR-039-the-market-local-product-tier.md">ADR-039</see>).
/// </para>
/// <para>
/// <b>Deliberately small.</b> What is inside it (S002), how it may be supplied
/// and how long it lasts (S003), and the artwork printed for it (S004) each
/// arrive with the story that reads them. What this one establishes is the
/// identity they hang from — the same discipline
/// <see cref="MedicinalProduct"/> was built with.
/// </para>
/// </remarks>
public sealed class PackagedProduct : AggregateRoot<PackagedProductId>
{
    public const int DescriptionMaxLength = 250;
    public const int PackCodeMaxLength = 50;

    private readonly List<PackageMarketingStatusEntry> _marketingStatusHistory
        = [];

    // Parameterless, with an object-initializer factory below: an owned value
    // object cannot bind to a constructor parameter, and PackSizeUnit is one.
    // The same shape MedicinalProductComponent and Ingredient use, and the trap
    // EPIC-010a's retro and EPIC-018 S001 both paid for.
    private PackagedProduct()
    {
    }

    /// <summary>The owning tenant (ADR-031). Fail-closed, set once.</summary>
    public TenantId TenantId { get; private set; } = default!;

    /// <summary>
    /// The market this pack is sold in. Immutable — repointing it would move a
    /// pack between jurisdictions, which is not an edit.
    /// </summary>
    public MedicinalProductId MedicinalProductId { get; private set; } = default!;

    /// <summary>
    /// What a person would read off the carton: <em>"Carton of 3 blisters × 10
    /// film-coated tablets"</em>.
    /// </summary>
    public string Description { get; private set; } = default!;

    /// <summary>
    /// How much is in it — <c>30</c>, against
    /// <see cref="PackSizeUnit"/>'s <em>tablet</em>.
    /// </summary>
    /// <remarks>
    /// Null is ordinary and means <em>not stated</em>, which a pack in design
    /// genuinely is. What is refused is <b>half</b> a size: a quantity with no
    /// unit could be tablets, millilitres or vials, and a unit with no quantity
    /// says nothing at all. The same guard shape as a population's age band
    /// (EPIC-018 S003).
    /// </remarks>
    public decimal? PackSizeQuantity { get; private set; }

    /// <summary>
    /// The unit the quantity counts, from the shared vocabulary
    /// <see cref="PharmaceuticalVocabulary.UnitsOfPresentation"/> — the same
    /// list a presentation and a component already use, not a fourth copy.
    /// </summary>
    public CodedConcept? PackSizeUnit { get; private set; }

    /// <summary>
    /// The market's own identifier for this pack — an NDC, a national code, a
    /// PZN. Null until the market issues one.
    /// </summary>
    /// <remarks>
    /// <b>Free text, and deliberately not validated by market.</b> Every
    /// jurisdiction formats its own, and a regex per country is a rule RegOS
    /// would be wrong about more often than a registrar. It is what the tenant
    /// supplied, which is the same claim <see cref="AtcCode"/> makes.
    /// </remarks>
    public string? PackCode { get; private set; }

    /// <summary>
    /// What is commercially true of this pack. Stored, not replayed, so the
    /// pack list reads one indexed column rather than reducing a history per
    /// row; <see cref="MarketingStatusHistory"/> records how it got here.
    /// </summary>
    public PackageMarketingStatus CurrentMarketingStatus { get; private set; }

    /// <summary>
    /// Who may hand this pack over — prescription only, pharmacy, general sale.
    /// Screen word: <b>Legal status</b>. Null until it is classified.
    /// </summary>
    /// <remarks>
    /// <b>On the pack, and that is the decision</b> (ADR-061 §1's discriminator
    /// again): a 16-tablet pack of paracetamol may be general sale where a
    /// 100-tablet pack of the same tablets is pharmacy-only. The restriction
    /// follows the quantity supplied, not the active substance, so it cannot
    /// live on the product or the presentation.
    /// <para>
    /// <b>Undated, deliberately.</b> A reclassification is a real regulatory
    /// event and nobody has asked to keep its history. If that changes, the
    /// shape is <see cref="PackageMarketingStatusEntry"/>'s exactly — which
    /// would make it the <em>fourth</em> identical status history, and
    /// therefore the demonstration
    /// <see href="../../../docs/adr/ADR-018-rule-of-three.md">ADR-018</see>
    /// asks for before the pattern is abstracted.
    /// </para>
    /// </remarks>
    public CodedConcept? LegalStatusOfSupply { get; private set; }

    /// <summary>
    /// How long the pack keeps and how it must be stored. Screen word:
    /// <b>Shelf life &amp; storage</b>.
    /// </summary>
    /// <remarks>
    /// <b>Never null.</b> A pack nobody has spoken about carries
    /// <see cref="ShelfLifeStorage.NotStated"/>, and
    /// <see cref="ShelfLifeStorage.IsStated"/> says which — a named question
    /// rather than a null check.
    /// </remarks>
    public ShelfLifeStorage ShelfLife { get; private set; } = ShelfLifeStorage.NotStated;

    /// <summary>
    /// Every marketing status this pack has held, oldest first. Append-only.
    /// </summary>
    public IReadOnlyCollection<PackageMarketingStatusEntry> MarketingStatusHistory
        => _marketingStatusHistory.AsReadOnly();

    public DateTime CreatedOnUtc { get; private set; }

    /// <param name="statusDate">
    /// The business date this pack came into being — supplied, never read from
    /// the clock, so a migrated portfolio can state when the pack actually
    /// existed.
    /// </param>
    public static PackagedProduct Create(
        TenantId tenantId,
        MedicinalProductId medicinalProductId,
        string description,
        decimal? packSizeQuantity,
        CodedConcept? packSizeUnit,
        string? packCode,
        DateOnly statusDate)
    {
        if (tenantId is null)
            throw new DomainException(PackagedProductErrors.TenantRequired);

        if (medicinalProductId is null)
            throw new DomainException(
                PackagedProductErrors.MedicinalProductRequired);

        if (statusDate == default)
            throw new DomainException(PackagedProductErrors.OccurredOnRequired);

        var pack = new PackagedProduct
        {
            Id = PackagedProductId.New(),
            TenantId = tenantId,
            MedicinalProductId = medicinalProductId,
            CreatedOnUtc = DateTime.UtcNow
        };

        pack.Describe(description, packSizeQuantity, packSizeUnit, packCode);

        // The first history entry is the status it starts in, not a separate
        // "created" event: one chronological sequence of the states held, in
        // one vocabulary. The same shape MedicinalProduct uses.
        pack.Record(PackageMarketingStatus.Planned, statusDate, null);

        return pack;
    }

    /// <summary>
    /// Restates what the pack is — description, size and code together.
    /// </summary>
    /// <remarks>
    /// <b>One method rather than four setters</b>, because the three facts are
    /// settled together: a corrected pack size that left the description saying
    /// <em>"carton of 30"</em> would be a pack that contradicts itself. The same
    /// reasoning that made <c>PrepareRevision</c> restate rather than patch
    /// (EPIC-018 S002).
    /// </remarks>
    public void Describe(
        string description,
        decimal? packSizeQuantity,
        CodedConcept? packSizeUnit,
        string? packCode)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException(PackagedProductErrors.DescriptionRequired);

        if (description.Trim().Length > DescriptionMaxLength)
            throw new DomainException(PackagedProductErrors.DescriptionTooLong);

        // Half a pack size is refused, and the message names the ambiguity
        // rather than the field.
        if (packSizeQuantity is not null && packSizeUnit is null)
            throw new DomainException(PackagedProductErrors.PackSizeUnitRequired);

        if (packSizeUnit is not null && packSizeQuantity is null)
            throw new DomainException(
                PackagedProductErrors.PackSizeQuantityRequired);

        if (packSizeQuantity is <= 0)
            throw new DomainException(
                PackagedProductErrors.PackSizeMustBePositive);

        if (packCode is not null && packCode.Trim().Length > PackCodeMaxLength)
            throw new DomainException(PackagedProductErrors.PackCodeTooLong);

        Description = description.Trim();
        PackSizeQuantity = packSizeQuantity;
        PackSizeUnit = packSizeUnit;
        PackCode = string.IsNullOrWhiteSpace(packCode) ? null : packCode.Trim();
    }

    /// <summary>
    /// Records who may hand this pack over. Null withdraws the classification.
    /// </summary>
    /// <remarks>
    /// <b>Its own method, not part of <see cref="StateShelfLife"/>.</b> The two
    /// facts move on different clocks — a reclassification is a regulatory
    /// decision, a shelf-life extension arrives by variation — and neither can
    /// make the other incoherent, which is the test <see cref="Describe"/> is
    /// grouped by. The application layer submits them together because one
    /// person states them in one sitting; the aggregate keeps them apart
    /// because they are two facts.
    /// </remarks>
    public void Classify(CodedConcept? legalStatusOfSupply)
    {
        LegalStatusOfSupply = legalStatusOfSupply;
    }

    /// <summary>
    /// States how long the pack keeps and how it must be stored.
    /// </summary>
    /// <remarks>
    /// <b>Takes the whole statement, never its parts.</b> There is no
    /// <c>SetShelfLifePeriod</c> beside a <c>SetStorageConditions</c>, because
    /// the period is only true under the conditions and two setters would let
    /// one be changed without the other. <see cref="ShelfLifeStorage"/> checks
    /// its own coherence in its factory; this method only refuses the absence
    /// of a statement, and <see cref="ShelfLifeStorage.NotStated"/> is how a
    /// caller withdraws one.
    /// </remarks>
    public void StateShelfLife(ShelfLifeStorage shelfLife)
    {
        if (shelfLife is null)
            throw new DomainException(PackagedProductErrors.ShelfLifeRequired);

        ShelfLife = shelfLife;
    }

    /// <summary>
    /// Records what became commercially true of this pack, and when.
    /// </summary>
    /// <remarks>
    /// <b>No transition table</b>, for the reason
    /// <see cref="MedicinalProduct.ChangeMarketStatus"/> gives: a pack may be
    /// withdrawn from sale and reintroduced, and none of that is incoherent.
    /// Two rules survive because they are about coherence rather than process —
    /// a status cannot be re-entered from itself, and business time moves
    /// forward — plus the one that is specific to
    /// <see cref="PackageMarketingStatus.Planned"/>.
    /// </remarks>
    public void ChangeMarketingStatus(
        PackageMarketingStatus target,
        DateOnly occurredOn,
        string? note = null)
    {
        if (!Enum.IsDefined(target))
            throw new DomainException(
                PackagedProductErrors.MarketingStatusNotRecognised);

        if (occurredOn == default)
            throw new DomainException(PackagedProductErrors.OccurredOnRequired);

        if (target == PackageMarketingStatus.Planned)
            throw new BusinessRuleViolationException(
                PackagedProductErrors.PackCannotBePlannedAgain);

        if (target == CurrentMarketingStatus)
            throw new BusinessRuleViolationException(
                PackagedProductErrors.AlreadyInMarketingStatus(target));

        // Max, not the last element: nothing orders what the database hands
        // back, and an invariant may not depend on the order its own storage
        // returns rows in. MedicinalProduct learned this the expensive way and
        // wrote it down; this is the copy that benefits.
        if (occurredOn < _marketingStatusHistory.Max(entry => entry.OccurredOn))
            throw new DomainException(
                PackagedProductErrors.OccurredOnBeforePreviousEntry);

        Record(target, occurredOn, note);
    }

    /// <summary>
    /// The core invariant: <b>every change updates
    /// <see cref="CurrentMarketingStatus"/> and appends exactly one immutable
    /// entry.</b> Nothing else writes either, so current state and the record of
    /// how it was reached can never disagree.
    /// </summary>
    private void Record(
        PackageMarketingStatus status,
        DateOnly occurredOn,
        string? note)
    {
        if (note is not null
            && note.Trim().Length > PackageMarketingStatusEntry.NoteMaxLength)
        {
            throw new DomainException(PackagedProductErrors.NoteTooLong);
        }

        CurrentMarketingStatus = status;

        _marketingStatusHistory.Add(new PackageMarketingStatusEntry(
            PackageMarketingStatusEntryId.New(),
            status,
            occurredOn,
            DateTime.UtcNow,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim()));
    }
}
