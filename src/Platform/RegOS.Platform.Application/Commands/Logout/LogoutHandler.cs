using RegOS.Platform.Application.Services;
using RegOS.Platform.Application.Authentication;
using RegOS.Platform.Domain.Aggregates.RefreshToken;
using RegOS.Platform.Domain.Aggregates.Session;

namespace RegOS.Platform.Application.Commands.Logout;

/// <summary>
/// Ends a session by revoking its refresh token.
/// </summary>
/// <remarks>
/// Never fails. An absent, unknown, expired or already-revoked token all mean
/// the same thing to the caller — you are signed out — and there is nothing
/// useful they could do with an error. Signing out must also be safe to retry,
/// and must not become a way to ask whether a token exists.
///
/// The access token is not revoked and cannot be: it is a signed statement, not
/// a database row, and it stays valid until it expires. That is the cost of
/// stateless tokens, and the reason they last fifteen minutes.
/// </remarks>
public sealed class LogoutHandler
{
    private readonly SessionRevoker _revoker;
    private readonly IRefreshTokenIssuer _issuer;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ISessionRepository _sessions;

    public LogoutHandler(
        SessionRevoker revoker,
        IRefreshTokenIssuer issuer,
        IRefreshTokenRepository refreshTokens,
        ISessionRepository sessions)
    {
        _revoker = revoker;
        _issuer = issuer;
        _refreshTokens = refreshTokens;
        _sessions = sessions;
    }

    public async Task HandleAsync(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken)) return;

        var token = await _refreshTokens.GetByHashAsync(
            _issuer.Hash(command.RefreshToken), cancellationToken);

        if (token is null) return;

        var now = DateTime.UtcNow;

        // The whole session, not only the token presented. Signing out ends the
        // sign-in; leaving the session row alive would keep this browser on the
        // user's own sessions page, listed as current, after they had left
        // (AUTH-010).
        var session = await _sessions.GetByIdAsync(
            token.SessionId, cancellationToken);

        if (session is not null)
        {
            await _revoker.RevokeAsync(session, now, cancellationToken);

            return;
        }

        token.Revoke(now);

        await _refreshTokens.UpdateAsync(token, cancellationToken);
    }
}
