namespace RegOS.Platform.Infrastructure.Authentication;

/// <summary>
/// Claim names RegOS puts in its own tokens. Kept in one place because the
/// issuer writes them and the authentication middleware will read them, and a
/// typo in either would be a silent authorization failure.
/// </summary>
public static class RegOSClaims
{
    /// <summary>
    /// The organization the user belongs to. This is what replaces the
    /// <c>X-Tenant-Id</c> header: once tokens are validated, tenant identity
    /// arrives as a signed claim rather than as something any caller can assert
    /// (ADR-013).
    /// </summary>
    public const string OrganizationId = "regos:organization_id";
}
