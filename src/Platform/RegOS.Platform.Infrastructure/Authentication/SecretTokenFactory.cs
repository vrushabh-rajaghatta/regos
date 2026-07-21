using System.Security.Cryptography;
using System.Text;

namespace RegOS.Platform.Infrastructure.Authentication;

/// <summary>
/// Mints high-entropy secrets and recognises them again.
/// </summary>
/// <remarks>
/// <para>
/// The one thing refresh tokens and invitations genuinely share: random
/// generation, a fast deterministic hash, and an encoding that survives a
/// cookie, a header and a URL. Everything else about them differs — refresh
/// tokens rotate and chain, invitations are consumed once — so those stay
/// separate aggregates with separate issuers, and only this is shared.
/// </para>
/// <para>
/// SHA-256, not the password hasher. A password is low-entropy and
/// human-chosen, so it needs a slow salted hash to survive offline guessing.
/// These values are 256 bits of RNG output: there is nothing to guess, so
/// slowness buys nothing, and a per-value salt would make the stored hash
/// impossible to look up — the one operation both stores exist for.
/// </para>
/// <para>
/// Concrete, with no interface. It hides no infrastructure choice from the
/// application layer; the issuers above it do that.
/// </para>
/// </remarks>
public sealed class SecretTokenFactory
{
    /// <summary>
    /// 256 bits. Enough that guessing is not a threat model, which is what
    /// makes a fast hash the right way to store one.
    /// </summary>
    private const int TokenBytes = 32;

    /// <summary>
    /// RandomNumberGenerator, not Random or Guid: this value is a credential,
    /// and the other two are predictable enough to forge.
    /// </summary>
    public string CreateValue() =>
        Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenBytes));

    public string Hash(string tokenValue) =>
        Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(tokenValue)));

    /// <summary>URL-safe and padding-free, so it needs no escaping anywhere.</summary>
    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
