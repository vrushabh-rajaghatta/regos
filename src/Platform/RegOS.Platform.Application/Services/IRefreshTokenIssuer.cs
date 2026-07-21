namespace RegOS.Platform.Application.Services;

/// <summary>
/// Creates refresh token values and recognises them again.
/// </summary>
/// <remarks>
/// Separate from <see cref="IPasswordHasher"/> because the two problems are
/// genuinely different, and using the password hasher here would not work:
/// <list type="bullet">
///   <item>A password is low-entropy and chosen by a human, so it needs a slow,
///   salted hash to survive an offline guessing attack.</item>
///   <item>A refresh token is 256 bits from a cryptographic RNG. There is
///   nothing to guess, so slowness buys nothing — and a per-value salt would
///   make the stored hash impossible to look up, which is the one operation
///   this type exists for.</item>
/// </list>
/// So this is a plain SHA-256 of the token value. Not a weaker choice than
/// PBKDF2 — a different one, for a secret with different properties.
/// </remarks>
public interface IRefreshTokenIssuer
{
    /// <summary>
    /// Mints a new token. The plaintext value is returned exactly once, to be
    /// handed to the client; only the hash is ever persisted.
    /// </summary>
    IssuedRefreshToken Issue(DateTime now);

    /// <summary>
    /// Hashes a value presented by a client so it can be looked up. Must agree
    /// with <see cref="Issue"/> or no token would ever be found again.
    /// </summary>
    string Hash(string tokenValue);
}

/// <param name="Value">The secret handed to the client. Never stored.</param>
/// <param name="Hash">What is stored in its place.</param>
/// <param name="ExpiresAt">When it stops being exchangeable.</param>
public sealed record IssuedRefreshToken(
    string Value,
    string Hash,
    DateTime ExpiresAt);
