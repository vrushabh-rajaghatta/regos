using RegOS.SharedKernel.Primitives;

namespace RegOS.Organization.Domain.Aggregates.Organization;

public sealed class OrganizationId : StronglyTypedId
{
    public OrganizationId(Guid value) : base(value)
    {
    }

    public static OrganizationId New() => new(Guid.NewGuid());

    public static OrganizationId From(Guid value) => new(value);

    public static implicit operator Guid(OrganizationId id) => id.Value;
}
