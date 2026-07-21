using RegOS.Platform.Domain.Aggregates.RefreshToken;
using RegOS.Platform.Domain.Aggregates.Session;
using RegOS.Platform.Domain.Aggregates.User;

using SessionAggregate = RegOS.Platform.Domain.Aggregates.Session.Session;

namespace RegOS.Platform.Application.Authentication;

/// <summary>
/// Ends sessions: one of them, or all of them.
/// </summary>
/// <remarks>
/// <para>
/// The counterpart to <see cref="SessionFactory"/>: one way to begin a session,
/// one way to end them. Unlike the token issuers this abstraction protects a
/// correctness property — a flow that forgets to call it leaves a compromised
/// session alive, which is a vulnerability rather than a repetition (ADR-018,
/// ADR-028).
/// </para>
/// <para>
/// Ending a session means two things, and doing only the first is the mistake
/// this class exists to make impossible: the session row is revoked <em>and</em>
/// every live refresh token carrying it is revoked. A revoked session whose
/// token still worked would be a sessions page that lies.
/// </para>
/// <para>
/// Deliberately one aggregate at a time rather than a bulk
/// <c>UPDATE … WHERE "RevokedOn" IS NULL</c>. That predicate is a second
/// implementation of the aggregates' own revocation rule — including its
/// promise to keep the first revocation time — written where the domain cannot
/// see it. They agree today; they would not survive the first change to either
/// (ADR-020).
/// </para>
/// </remarks>
public sealed class SessionRevoker
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ISessionRepository _sessions;

    public SessionRevoker(
        IRefreshTokenRepository refreshTokens,
        ISessionRepository sessions)
    {
        _refreshTokens = refreshTokens;
        _sessions = sessions;
    }

    /// <summary>Ends every live session a user has.</summary>
    public async Task RevokeEveryFor(
        UserId userId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        foreach (var session in await _sessions.GetActiveForUserAsync(
            userId, cancellationToken))
        {
            await RevokeAsync(session, now, cancellationToken);
        }

        // Belt and braces, and not redundant: tokens issued before AUTH-010
        // have no session, and a token orphaned by any future bug would
        // otherwise outlive the sweep it was meant to be caught by.
        foreach (var token in await _refreshTokens.GetActiveForUserAsync(
            userId, cancellationToken))
        {
            token.Revoke(now);

            await _refreshTokens.UpdateAsync(token, cancellationToken);
        }
    }

    /// <summary>Ends one session, and the tokens carrying it.</summary>
    public async Task RevokeAsync(
        SessionAggregate session,
        DateTime now,
        CancellationToken cancellationToken)
    {
        session.Revoke(now);

        await _sessions.UpdateAsync(session, cancellationToken);

        foreach (var token in await _refreshTokens.GetActiveForSessionAsync(
            session.Id, cancellationToken))
        {
            token.Revoke(now);

            await _refreshTokens.UpdateAsync(token, cancellationToken);
        }
    }
}
