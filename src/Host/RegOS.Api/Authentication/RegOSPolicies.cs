namespace RegOS.Api.Authentication;

/// <summary>
/// Named authorization policies (ADR-033). Endpoint gating happens through
/// these, never through role checks inside handlers — a handler that runs is
/// already authorized, exactly as a claim that is present is already verified.
/// </summary>
/// <remarks>
/// The two policies are exact-match on purpose. A platform administrator does
/// NOT satisfy the tenant-administrator policy: they have no tenant, so every
/// tenant-scoped endpoint would throw at <c>ITenantContext</c> anyway — the
/// policy just says so with a 403 instead of a confusing 401. Hierarchical
/// roles arrive when a feature needs them, not before.
/// </remarks>
public static class RegOSPolicies
{
    public const string PlatformAdministrator = "PlatformAdministrator";

    public const string TenantAdministrator = "TenantAdministrator";
}
