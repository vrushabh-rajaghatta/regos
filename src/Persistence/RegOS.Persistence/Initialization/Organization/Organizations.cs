using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Primitives;

using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Persistence.Initialization.Organization;

/// <summary>
/// Each demo organization is owned by its same-guid tenant — the mirror-entry
/// convention (ADR-032): the organization whose id equals a tenant's id is
/// that tenant's own company, the first entry in its registry.
/// </summary>
internal static class Organizations
{
    public static IReadOnlyList<OrganizationAggregate> Data =>
    [
        OrganizationAggregate.Create(
            new OrganizationId(
                OrganizationIds.DemoManufacturer),
            new TenantId(OrganizationIds.DemoManufacturer),
            "Demo Manufacturer Ltd.",
            OrganizationType.Manufacturer),
        OrganizationAggregate.Create(
            new OrganizationId(
                OrganizationIds.DemoSponsor),
            new TenantId(OrganizationIds.DemoSponsor),
            "Demo Sponsor Ltd.",
            OrganizationType.Sponsor),
        OrganizationAggregate.Create(
            new OrganizationId(
                OrganizationIds.DemoMarketingAuthorizationHolder),
            new TenantId(OrganizationIds.DemoMarketingAuthorizationHolder),
            "Demo MAH Ltd.",
            OrganizationType.MarketingAuthorizationHolder)
    ];
}
