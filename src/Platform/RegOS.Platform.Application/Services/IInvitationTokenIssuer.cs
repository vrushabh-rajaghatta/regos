namespace RegOS.Platform.Application.Services;

/// <summary>
/// Creates invitation tokens and recognises them again. Deliberately the same
/// shape as <see cref="IRefreshTokenIssuer"/> — they share a generator and a
/// hash underneath — but a separate interface, because the two differ in the
/// only thing an issuer decides: how long the secret lives.
/// </summary>
public interface IInvitationTokenIssuer
{
    /// <summary>
    /// Mints a token. The plaintext is returned exactly once, to be put in the
    /// acceptance link; only the hash is persisted.
    /// </summary>
    IssuedInvitationToken Issue(DateTime now);

    /// <summary>
    /// Hashes a value presented by a client so it can be looked up. Must agree
    /// with <see cref="Issue"/> or no invitation would ever be found again.
    /// </summary>
    string Hash(string tokenValue);
}

/// <param name="Value">The secret that goes in the link. Never stored.</param>
/// <param name="Hash">What is stored in its place.</param>
/// <param name="ExpiresAt">When the invitation stops being acceptable.</param>
public sealed record IssuedInvitationToken(
    string Value,
    string Hash,
    DateTime ExpiresAt);
