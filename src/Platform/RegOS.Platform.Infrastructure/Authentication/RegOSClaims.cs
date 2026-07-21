namespace RegOS.Platform.Infrastructure.Authentication;

/// <summary>
/// Claim names RegOS puts in its own tokens. Kept in one place because the
/// issuer writes them and the authentication middleware will read them, and a
/// typo in either would be a silent authorization failure.
/// </summary>
public static class RegOSClaims
{
    /// <summary>
    /// The tenant the user belongs to, and the source of tenant identity
    /// for the whole platform. It replaced the <c>X-Tenant-Id</c> header, which
    /// is deleted: tenancy is now signed rather than asserted (ADR-024). Named
    /// <c>regos:organization_id</c> until ADR-030 separated tenant from
    /// organization; renaming the claim invalidated every issued token, which
    /// was accepted while the only account was the development seeder's.
    /// </summary>
    public const string TenantId = "regos:tenant_id";

    /// <summary>
    /// The user's role, as the enum member's name. The issuer once refused
    /// role claims on principle ("identity, not an authorization snapshot");
    /// ADR-033 reverses that deliberately: staleness is capped at the
    /// fifteen-minute token lifetime, a demotion can end sessions through the
    /// ADR-028 machinery, and the alternative was a database read on every
    /// authorization check.
    /// </summary>
    public const string Role = "regos:role";
}
