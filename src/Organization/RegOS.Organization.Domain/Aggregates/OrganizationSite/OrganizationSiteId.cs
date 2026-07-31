using RegOS.SharedKernel.Primitives;

namespace RegOS.Organization.Domain.Aggregates.OrganizationSite;

public sealed class OrganizationSiteId : StronglyTypedId
{
    public OrganizationSiteId(Guid value) : base(value)
    {
    }

    public static OrganizationSiteId New() => new(Guid.NewGuid());

    public static OrganizationSiteId From(Guid value) => new(value);

    public static implicit operator Guid(OrganizationSiteId id) => id.Value;
}

public sealed class SiteIdentifierId : StronglyTypedId
{
    public SiteIdentifierId(Guid value) : base(value)
    {
    }

    public static SiteIdentifierId New() => new(Guid.NewGuid());

    public static implicit operator Guid(SiteIdentifierId id) => id.Value;
}
