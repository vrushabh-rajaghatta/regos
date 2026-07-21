using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;

using RegOS.Platform.Application.Services;

namespace RegOS.Platform.Infrastructure.Authentication;

/// <summary>
/// Generates refresh tokens and hashes them. Every cryptographic operation here
/// is a single call to a framework primitive: no custom alphabet, no hand-rolled
/// mixing, no home-made encoding.
/// </summary>
public sealed class RefreshTokenIssuer : IRefreshTokenIssuer
{
    /// <summary>
    /// 256 bits. Enough that guessing is not a threat model, which is what
    /// makes a fast hash the right choice for storing it.
    /// </summary>
    private const int TokenBytes = 32;

    private readonly RefreshTokenOptions _options;

    public RefreshTokenIssuer(IOptions<RefreshTokenOptions> options)
    {
        _options = options.Value;
    }

    public IssuedRefreshToken Issue(DateTime now)
    {
        // RandomNumberGenerator, not Random or Guid: this value is a
        // credential, and the other two are predictable enough to forge.
        var value = Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenBytes));

        return new IssuedRefreshToken(
            value,
            Hash(value),
            now.AddDays(_options.Days));
    }

    public string Hash(string tokenValue) =>
        Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(tokenValue)));

    /// <summary>
    /// URL-safe and padding-free, so the value survives a cookie, a header and
    /// a query string without escaping.
    /// </summary>
    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
