using RegOS.Platform.Domain.Aggregates.RefreshToken;
using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Authentication;

/// <summary>
/// Ends every live session a user has.
/// </summary>
/// <remarks>
/// <para>
/// The counterpart to <see cref="SessionFactory"/>: one way to begin a session,
/// one way to end all of them. Extracted when the third caller appeared, and
/// unlike the token issuers this abstraction protects a correctness property —
/// a flow that forgets to call it leaves a compromised session alive, which is
/// a vulnerability rather than a repetition (ADR-018, ADR-028).
/// </para>
/// <para>
/// Note what this is <em>not</em>: revoking the single token a caller
/// presented. That is sign-out, it lives in <c>LogoutHandler</c>, and folding
/// the two together would produce one class with a flag deciding how much to
/// destroy.
/// </para>
/// <para>
/// Deliberately one aggregate at a time rather than a bulk
/// <c>UPDATE … WHERE "RevokedOn" IS NULL</c>. That predicate is a second
/// implementation of <see cref="RefreshToken.Revoke"/>'s rule — including its
/// promise to keep the first revocation time — written somewhere the domain
/// cannot see it. They agree today; they would not survive the first change to
/// either (ADR-020). If the round trips ever matter, the fix goes through the
/// aggregate, not around it.
/// </para>
/// </remarks>
public sealed class SessionRevoker
{
    private readonly IRefreshTokenRepository _refreshTokens;

    public SessionRevoker(IRefreshTokenRepository refreshTokens)
    {
        _refreshTokens = refreshTokens;
    }

    public async Task RevokeEveryFor(
        UserId userId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var active = await _refreshTokens.GetActiveForUserAsync(
            userId, cancellationToken);

        foreach (var token in active)
        {
            token.Revoke(now);

            await _refreshTokens.UpdateAsync(token, cancellationToken);
        }
    }
}
