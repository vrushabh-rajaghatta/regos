namespace RegOS.Platform.Infrastructure.Authentication;

/// <summary>
/// JWT signing configuration, bound from the <c>Jwt</c> configuration section.
///
/// There is deliberately no default for <see cref="SigningKey"/> and no
/// generated fallback. A missing key fails startup rather than silently
/// producing tokens signed with something predictable — the failure mode that
/// makes a development shortcut reach production unnoticed.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// HMAC-SHA256 requires at least 256 bits of key material; a shorter key
    /// is rejected by the library at signing time, which would turn a
    /// configuration mistake into a runtime failure on the first login instead
    /// of a startup failure.
    /// </summary>
    public const int MinimumSigningKeyBytes = 32;

    public string? SigningKey { get; set; }

    public string? Issuer { get; set; }

    public string? Audience { get; set; }

    public int AccessTokenMinutes { get; set; } = 15;
}
