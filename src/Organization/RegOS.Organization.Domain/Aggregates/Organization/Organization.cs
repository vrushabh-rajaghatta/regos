using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Organization.Domain.Aggregates.Organization;

public sealed class Organization : AggregateRoot<OrganizationId>
{
    public const string TenantRequired =
        "An organization must belong to a tenant.";

    private readonly List<OrganizationIdentifier> _identifiers = [];

    private Organization()
    {
    }

    /// <summary>
    /// The tenant whose registry this organization belongs to (ADR-032).
    /// Organizations are a tenant's business relationships — visible to no
    /// other tenant. The one whose id equals the owning tenant's id is the
    /// tenant's own company, its mirror entry.
    /// </summary>
    public TenantId TenantId { get; private set; } = default!;

    public string LegalName { get; private set; } = default!;

    public OrganizationType Type { get; private set; }

    public OrganizationStatus Status { get; private set; }

    /// <summary>
    /// The business date the current status took effect. Completes the
    /// activation-toggle shape the other organizational entities carry —
    /// leaving one of three dateless would read as an oversight rather than a
    /// decision.
    /// </summary>
    public DateOnly StatusDate { get; private set; }

    /// <summary>Trading name or short form — "Demo MAH", "MHRA".</summary>
    public string? Acronym { get; private set; }

    /// <summary>
    /// The registered name in the local script, where it differs from the
    /// romanised legal name.
    /// </summary>
    public string? NameNativeLanguage { get; private set; }

    /// <summary>
    /// Every registry identifier this company holds — DUNS, VAT, company
    /// registration. See <see cref="OrganizationIdentifier"/> for why this
    /// duplicates <c>SiteIdentifier</c> rather than sharing with it.
    /// </summary>
    public IReadOnlyCollection<OrganizationIdentifier> Identifiers
        => _identifiers.AsReadOnly();

    public static Organization Create(
        OrganizationId id,
        TenantId tenantId,
        string legalName,
        OrganizationType type,
        DateOnly? statusDate = null)
        => new()
        {
            Id = id,
            TenantId = tenantId
                ?? throw new DomainException(TenantRequired),
            LegalName = NormalizeLegalName(legalName),
            Type = Validated(type),
            Status = OrganizationStatus.Active,
            // Optional, unlike Site and Contact, because this factory predates
            // the field and every existing caller is a UI action happening now.
            // An importer states the real date.
            StatusDate = statusDate ?? DateOnly.FromDateTime(DateTime.UtcNow)
        };

    public static Organization Create(
        TenantId tenantId,
        string legalName,
        OrganizationType type,
        DateOnly? statusDate = null)
        => Create(OrganizationId.New(), tenantId, legalName, type, statusDate);

    /// <summary>
    /// Corrects the registered legal name. Permitted while inactive: retiring an
    /// organization says "do not start new work with this", not "freeze the
    /// record", and a misspelled legal name is worth fixing either way. This
    /// matches Product, where an archived product can still be renamed.
    /// </summary>
    public void Rename(string? legalName)
        => LegalName = NormalizeLegalName(legalName);

    /// <summary>
    /// Reclassifies the organization. Separate from <see cref="Rename"/> rather
    /// than one UpdateDetails: the two carry different intent and will grow
    /// different rules — reclassifying an organization that already holds
    /// marketing authorizations is a conversation we have not had yet, and an
    /// explicit method is where that rule will live when we do.
    /// </summary>
    public void Reclassify(OrganizationType type) => Type = Validated(type);

    /// <summary>Records the company's short form and local-script name.</summary>
    public void DescribeAs(string? acronym, string? nameNativeLanguage)
    {
        Acronym = string.IsNullOrWhiteSpace(acronym) ? null : acronym.Trim();

        NameNativeLanguage = string.IsNullOrWhiteSpace(nameNativeLanguage)
            ? null
            : nameNativeLanguage.Trim();
    }

    /// <summary>
    /// Records an identifier this company is known by in some registry. One per
    /// scheme: a second VAT number would mean one of them is wrong.
    /// </summary>
    public OrganizationIdentifier AddIdentifier(
        IdentifierSchemeId schemeId,
        string value)
    {
        if (_identifiers.Any(x => x.SchemeId == schemeId))
            throw new BusinessRuleViolationException(
                OrganizationErrors.IdentifierSchemeAlreadyRecorded);

        var identifier = new OrganizationIdentifier(
            OrganizationIdentifierId.New(), schemeId, value);

        _identifiers.Add(identifier);

        return identifier;
    }

    public void RemoveIdentifier(OrganizationIdentifierId identifierId)
    {
        var identifier = _identifiers.FirstOrDefault(x => x.Id == identifierId)
            ?? throw new NotFoundException(OrganizationErrors.IdentifierNotFound);

        _identifiers.Remove(identifier);
    }

    private static string NormalizeLegalName(string? legalName)
        => string.IsNullOrWhiteSpace(legalName)
            ? throw new DomainException(OrganizationErrors.LegalNameRequired)
            : legalName.Trim();

    // Model binding happily turns {"type": 99} into an OrganizationType, so
    // without this an organization persists with a type that has no name.
    // Decidable from the request alone, therefore 400 (ADR-009).
    private static OrganizationType Validated(OrganizationType type)
        => Enum.IsDefined(type)
            ? type
            : throw new DomainException(OrganizationErrors.TypeInvalid);

    /// <summary>
    /// Retires the organization. It stays readable and its users and regulatory
    /// work are untouched — deactivating says "do not start anything new with
    /// this", not "pretend it never existed".
    /// </summary>
    public void Deactivate(DateOnly? on = null)
    {
        // Valid request, business state forbids it: 409, not a silent no-op
        // (ADR-009). A caller deactivating twice has a stale view of the world
        // and should be told so.
        if (Status == OrganizationStatus.Inactive)
            throw new BusinessRuleViolationException(
                OrganizationErrors.AlreadyInactive);

        Status = OrganizationStatus.Inactive;
        StatusDate = on ?? DateOnly.FromDateTime(DateTime.UtcNow);
    }

    /// <summary>
    /// Returns the organization to service. The mirror of
    /// <see cref="Deactivate"/>, and rejected the same way when there is no
    /// transition to make.
    /// </summary>
    public void Activate(DateOnly? on = null)
    {
        if (Status == OrganizationStatus.Active)
            throw new BusinessRuleViolationException(
                OrganizationErrors.AlreadyActive);

        Status = OrganizationStatus.Active;
        StatusDate = on ?? DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
