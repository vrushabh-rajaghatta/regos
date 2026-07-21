using RegOS.SharedKernel.Primitives;

using TenantAggregate = RegOS.Platform.Domain.Aggregates.Tenant.Tenant;

namespace RegOS.Persistence.Initialization.Platform;

internal static class Tenants
{
    public static IReadOnlyList<TenantAggregate> Data =>
    [
        TenantAggregate.Create(
            new TenantId(TenantIds.DemoManufacturer),
            "Demo Manufacturer Ltd."),
        TenantAggregate.Create(
            new TenantId(TenantIds.DemoSponsor),
            "Demo Sponsor Ltd."),
        TenantAggregate.Create(
            new TenantId(TenantIds.DemoMarketingAuthorizationHolder),
            "Demo MAH Ltd.")
    ];
}
