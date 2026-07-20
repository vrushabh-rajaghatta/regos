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

    public void Deactivate()
    {
        Status = OrganizationStatus.Inactive;
    }

    public void Activate()
    {
        Status = OrganizationStatus.Active;
    }
}
