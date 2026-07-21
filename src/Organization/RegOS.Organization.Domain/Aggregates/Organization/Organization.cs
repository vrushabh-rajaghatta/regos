using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Organization.Domain.Aggregates.Organization;

public sealed class Organization : AggregateRoot<OrganizationId>
{
    private Organization()
    {
    }

    public string LegalName { get; private set; } = default!;

    public OrganizationType Type { get; private set; }

    public OrganizationStatus Status { get; private set; }

    public static Organization Create(
        OrganizationId id,
        string legalName,
        OrganizationType type)
        => new()
        {
            Id = id,
            LegalName = NormalizeLegalName(legalName),
            Type = Validated(type),
            Status = OrganizationStatus.Active
        };

    public static Organization Create(
        string legalName,
        OrganizationType type)
        => Create(OrganizationId.New(), legalName, type);

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
    public void Deactivate()
    {
        // Valid request, business state forbids it: 409, not a silent no-op
        // (ADR-009). A caller deactivating twice has a stale view of the world
        // and should be told so.
        if (Status == OrganizationStatus.Inactive)
            throw new BusinessRuleViolationException(
                OrganizationErrors.AlreadyInactive);

        Status = OrganizationStatus.Inactive;
    }

    /// <summary>
    /// Returns the organization to service. The mirror of
    /// <see cref="Deactivate"/>, and rejected the same way when there is no
    /// transition to make.
    /// </summary>
    public void Activate()
    {
        if (Status == OrganizationStatus.Active)
            throw new BusinessRuleViolationException(
                OrganizationErrors.AlreadyActive);

        Status = OrganizationStatus.Active;
    }
}
