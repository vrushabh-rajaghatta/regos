namespace RegOS.Platform.Infrastructure.Authentication;

/// <summary>
/// Refresh token lifetime, bound from the <c>RefreshToken</c> configuration
/// section.
///
/// Deliberately not part of <see cref="JwtOptions"/>: a refresh token is not a
/// JWT, is not signed, and shares none of that type's settings. Keeping them
/// apart means neither can be changed in the belief that it affects the other.
/// </summary>
public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    /// <summary>
    /// How long a session survives without re-entering a password. There is a
    /// default because, unlike a signing key, a wrong value here is a usability
    /// decision rather than a security hole — and every rotation restarts it.
    /// </summary>
    public int Days { get; set; } = 14;
}
