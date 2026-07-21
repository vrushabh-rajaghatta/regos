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
    {
        if (string.IsNullOrWhiteSpace(legalName))
            throw new DomainException(OrganizationErrors.LegalNameRequired);

        // Model binding happily turns {"type": 99} into an OrganizationType,
        // so without this an organization persists with a type that has no
        // name. Decidable from the request alone, therefore 400 (ADR-009).
        if (!Enum.IsDefined(type))
            throw new DomainException(OrganizationErrors.TypeInvalid);

        return new Organization
        {
            Id = id,
            LegalName = legalName.Trim(),
            Type = type,
            Status = OrganizationStatus.Active
        };
    }

    public static Organization Create(
        string legalName,
        OrganizationType type)
        => Create(OrganizationId.New(), legalName, type);

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

    public void Activate()
    {
        Status = OrganizationStatus.Active;
    }
}
