namespace RegOS.Persistence.Initialization.Platform;

/// <summary>
/// The demo tenants share their guids with the demo organizations of the same
/// name. Not an accident: the AddTenants migration backfilled Tenants from
/// Organizations preserving ids (ADR-030), so a database migrated from the
/// fused model and a database seeded fresh agree on every id — and
/// <c>DevelopmentCredentialSeeder</c> keeps working against both.
/// </summary>
internal static class TenantIds
{
    public static readonly Guid DemoManufacturer =
        Guid.Parse("30000000-0000-0000-0000-000000000001");

    public static readonly Guid DemoSponsor =
        Guid.Parse("30000000-0000-0000-0000-000000000002");

    public static readonly Guid DemoMarketingAuthorizationHolder =
        Guid.Parse("30000000-0000-0000-0000-000000000003");
}
