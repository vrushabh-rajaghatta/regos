using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Primitives;

using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Persistence.Initialization.Organization;

/// <summary>
/// Each demo organization is owned by a tenant (ADR-032) and seeded with that
/// tenant's guid.
/// </summary>
/// <remarks>
/// The shared guid is a fixture artifact, not a convention: it dates from the
/// retired mirror-entry rule and survives only because seeded applications
/// cite these ids as applicant (ADR-060). Nothing may read meaning into an
/// organization id that matches a tenant id, here or anywhere.
/// </remarks>
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
