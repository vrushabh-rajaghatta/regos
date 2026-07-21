namespace RegOS.Platform.Application.Services;

/// <summary>
/// Creates password reset tokens and recognises them again. The third interface
/// of this shape, after <see cref="IRefreshTokenIssuer"/> and
/// <see cref="IInvitationTokenIssuer"/>, and still separate for the same reason
/// they are: they share a generator and a hash underneath, but differ in the
/// only thing an issuer decides — how long the secret lives. Whether three is
/// now enough evidence to unify them is a question for the AUTH-008
/// retrospective, not for this file.
/// </summary>
public interface IPasswordResetTokenIssuer
{
    /// <summary>
    /// Mints a token. The plaintext is returned exactly once, to be put in the
    /// reset link; only the hash is persisted.
    /// </summary>
    IssuedPasswordResetToken Issue(DateTime now);

    /// <summary>
    /// Hashes a value presented by a client so it can be looked up. Must agree
    /// with <see cref="Issue"/> or no reset would ever be found again.
    /// </summary>
    string Hash(string tokenValue);
}

/// <param name="Value">The secret that goes in the link. Never stored.</param>
/// <param name="Hash">What is stored in its place.</param>
/// <param name="ExpiresAt">When the reset stops being redeemable.</param>
public sealed record IssuedPasswordResetToken(
    string Value,
    string Hash,
    DateTime ExpiresAt);
