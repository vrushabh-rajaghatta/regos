using RegOS.Organization.Domain.Aggregates.Organization;
using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Persistence.Initialization.Organization;

internal static class Organizations
{
    public static IReadOnlyList<OrganizationAggregate> Data =>
    [
        OrganizationAggregate.Create(
            new OrganizationId(
                OrganizationIds.DemoManufacturer),
            "Demo Manufacturer Ltd.",
            OrganizationType.Manufacturer),
        OrganizationAggregate.Create(
            new OrganizationId(
                OrganizationIds.DemoSponsor),
            "Demo Sponsor Ltd.",
            OrganizationType.Sponsor),
        OrganizationAggregate.Create(
            new OrganizationId(
                OrganizationIds.DemoMarketingAuthorizationHolder),
            "Demo MAH Ltd.",
            OrganizationType.MarketingAuthorizationHolder)
    ];
}
