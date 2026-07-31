using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Registration.Domain.Aggregates.Registration;

/// <summary>
/// A product's authorisation to be marketed in one jurisdiction — what the
/// business <em>holds</em>, as distinct from what it <em>does</em> (a
/// RegulatoryApplication) or what it <em>sends</em> (a Submission).
/// </summary>
/// <remarks>
/// An aggregate root rather than a child of Product: it has an identity
/// outsiders quote (the registration number), a lifecycle of its own, and is
/// queried across the portfolio — "what do we hold in Canada?" — rather than
/// within a single product.
/// <para>
/// Several registrations may legitimately exist for the same product in the
/// same market: different strengths, presentations, holders after a partial
/// divestment, or legacy authorisations never surrendered. Nothing here
/// enforces one per market.
/// </para>
/// </remarks>
public sealed class Registration
{
    public const int RegistrationNumberMaxLength = 100;

    private readonly List<RegistrationStatusEntry> _history = [];

    private Registration(
        RegistrationId id,
        TenantId tenantId,
        ProductId productId,
        CountryId countryId,
        AuthorityId authorityId,
        OrganizationId holderOrganizationId,
        RegulatoryApplicationId? originatingApplicationId,
        DateTime createdOn)
    {
        Id = id;
        TenantId = tenantId;
        ProductId = productId;
        CountryId = countryId;
        AuthorityId = authorityId;
        HolderOrganizationId = holderOrganizationId;
        OriginatingApplicationId = originatingApplicationId;
        CurrentStatus = RegistrationStatus.Planned;
        CreatedOn = createdOn;
    }

    public RegistrationId Id { get; }

    /// <summary>
    /// The owning tenant — whose record this is. Not the same concept as
    /// <see cref="HolderOrganizationId"/>: an authorisation can be held on a
    /// partner's behalf (ADR-030).
    /// </summary>
    public TenantId TenantId { get; }

    public ProductId ProductId { get; }

    public CountryId CountryId { get; }

    public AuthorityId AuthorityId { get; }

    /// <summary>The marketing-authorisation holder.</summary>
    public OrganizationId HolderOrganizationId { get; private set; }

    /// <summary>
    /// The filing that produced this authorisation, when RegOS witnessed it.
    /// Null for acquired, in-licensed or migrated portfolios — RegOS must not
    /// assume it saw every regulatory event, and an authorisation whose filing
    /// happened elsewhere is no less real.
    /// </summary>
    public RegulatoryApplicationId? OriginatingApplicationId { get; }

    /// <summary>
    /// The authority's number for this authorisation. Null until granted; it is
    /// the registration's business identity once it exists.
    /// </summary>
    public string? RegistrationNumber { get; private set; }

    /// <summary>
    /// Stored, not replayed: the portfolio views read one indexed column rather
    /// than reducing a history per row. <see cref="History"/> records how it
    /// reached this.
    /// </summary>
    public RegistrationStatus CurrentStatus { get; private set; }

    public DateOnly? ApprovedOn { get; private set; }

    public DateOnly? ExpiresOn { get; private set; }

    public DateTime CreatedOn { get; }

    /// <summary>
    /// Every status this registration has held, oldest first. Append-only.
    /// </summary>
    public IReadOnlyCollection<RegistrationStatusEntry> History
        => _history.AsReadOnly();

    /// <param name="occurredOn">
    /// The business date this registration entered its first status. Supplied
    /// rather than read from the clock so a portfolio migrated from a legacy
    /// register can state when things actually happened.
    /// </param>
    /// <param name="originatingApplicationId">
    /// Optional — see <see cref="OriginatingApplicationId"/>.
    /// </param>
    public static Registration Create(
        TenantId tenantId,
        ProductId productId,
        CountryId countryId,
        AuthorityId authorityId,
        OrganizationId holderOrganizationId,
        DateOnly occurredOn,
        RegulatoryApplicationId? originatingApplicationId = null,
        string? note = null)
    {
        if (tenantId is null)
            throw new DomainException(RegistrationErrors.TenantRequired);

        if (productId == default)
            throw new DomainException(RegistrationErrors.ProductRequired);

        if (countryId == default)
            throw new DomainException(RegistrationErrors.CountryRequired);

        if (authorityId == default)
            throw new DomainException(RegistrationErrors.AuthorityRequired);

        if (holderOrganizationId == default)
            throw new DomainException(
                RegistrationErrors.HolderOrganizationRequired);

        if (occurredOn == default)
            throw new DomainException(RegistrationErrors.OccurredOnRequired);

        var registration = new Registration(
            RegistrationId.New(),
            tenantId,
            productId,
            countryId,
            authorityId,
            holderOrganizationId,
            originatingApplicationId,
            DateTime.UtcNow);

        // The first history entry is the status it starts in, not a separate
        // "created" event: the history is one chronological sequence of the
        // states held, in one vocabulary.
        registration.Record(RegistrationStatus.Planned, occurredOn, note);

        return registration;
    }

    /// <summary>
    /// Records that the authority granted this authorisation.
    /// </summary>
    /// <remarks>
    /// The only route to <see cref="RegistrationStatus.Approved"/>. Creation
    /// deliberately cannot start there: a privileged constructor that could
    /// materialise any state would let every rule guarding it be skipped. An
    /// import is create-then-record, and the history then reads honestly —
    /// recorded today, occurred in 2019.
    /// </remarks>
    /// <param name="approvedOn">
    /// The date the authority granted it — the business date, never the clock.
    /// </param>
    public void RecordApproval(
        string registrationNumber,
        DateOnly approvedOn,
        DateOnly? expiresOn = null,
        string? note = null)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
            throw new DomainException(
                RegistrationErrors.RegistrationNumberRequired);

        if (registrationNumber.Trim().Length > RegistrationNumberMaxLength)
            throw new DomainException(
                RegistrationErrors.RegistrationNumberTooLong);

        if (approvedOn == default)
            throw new DomainException(RegistrationErrors.OccurredOnRequired);

        if (CurrentStatus == RegistrationStatus.Approved)
            throw new BusinessRuleViolationException(
                RegistrationErrors.ApprovalAlreadyRecorded);

        if (expiresOn is { } expiry && expiry < approvedOn)
            throw new DomainException(RegistrationErrors.ExpiryBeforeApproval);

        RegistrationNumber = registrationNumber.Trim();
        ApprovedOn = approvedOn;
        ExpiresOn = expiresOn;

        Record(RegistrationStatus.Approved, approvedOn, note);
    }

    private void Record(
        RegistrationStatus status,
        DateOnly occurredOn,
        string? note)
    {
        if (note is not null
            && note.Trim().Length > RegistrationStatusEntry.NoteMaxLength)
        {
            throw new DomainException(RegistrationErrors.NoteTooLong);
        }

        CurrentStatus = status;

        _history.Add(new RegistrationStatusEntry(
            RegistrationStatusEntryId.New(),
            status,
            occurredOn,
            DateTime.UtcNow,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim()));
    }
}
