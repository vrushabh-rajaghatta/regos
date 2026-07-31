using RegOS.SharedKernel.Primitives;

namespace RegOS.Organization.Domain.Aggregates.OrganizationDivision;

public sealed class OrganizationDivisionId : StronglyTypedId
{
    public OrganizationDivisionId(Guid value) : base(value)
    {
    }

    public static OrganizationDivisionId New() => new(Guid.NewGuid());

    public static OrganizationDivisionId From(Guid value) => new(value);

    public static implicit operator Guid(OrganizationDivisionId id) => id.Value;
}
