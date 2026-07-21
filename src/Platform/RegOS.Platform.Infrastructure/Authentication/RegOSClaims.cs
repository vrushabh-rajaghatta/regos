namespace RegOS.Platform.Infrastructure.Authentication;

/// <summary>
/// Claim names RegOS puts in its own tokens. Kept in one place because the
/// issuer writes them and the authentication middleware will read them, and a
/// typo in either would be a silent authorization failure.
/// </summary>
public static class RegOSClaims
{
    /// <summary>
    /// The organization the user belongs to, and the source of tenant identity
    /// for the whole platform. It replaced the <c>X-Tenant-Id</c> header, which
    /// is deleted: tenancy is now signed rather than asserted (ADR-024).
    /// </summary>
    public const string OrganizationId = "regos:organization_id";
}
