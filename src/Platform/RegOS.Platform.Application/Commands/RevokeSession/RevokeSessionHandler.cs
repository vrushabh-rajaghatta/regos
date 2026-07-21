using RegOS.Platform.Application.Authentication;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.RefreshToken;
using RegOS.Platform.Domain.Aggregates.Session;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Application.Commands.RevokeSession;

/// <summary>
/// Ends one of the caller's sessions, or all the others.
/// </summary>
/// <remarks>
/// <para>
/// The ownership check is the whole security content of this handler. A session
/// id is a guid a caller supplies, so it must be proven to belong to them
/// before anything is revoked — otherwise this is an endpoint for signing other
/// people out.
/// </para>
/// <para>
/// A session belonging to someone else answers 404, not 403. "That is not
/// yours" tells a stranger the id was real, which is the same oracle ADR-022
/// closed at sign-in.
/// </para>
/// </remarks>
public sealed class RevokeSessionHandler
{
    private readonly SessionRevoker _revoker;
    private readonly ICurrentUser _currentUser;
    private readonly IRefreshTokenIssuer _issuer;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ISessionRepository _sessions;

    public RevokeSessionHandler(
        SessionRevoker revoker,
        ICurrentUser currentUser,
        IRefreshTokenIssuer issuer,
        IRefreshTokenRepository refreshTokens,
        ISessionRepository sessions)
    {
        _revoker = revoker;
        _currentUser = currentUser;
        _issuer = issuer;
        _refreshTokens = refreshTokens;
        _sessions = sessions;
    }

    /// <summary>
    /// Returns whether the caller just ended their own current session, so the
    /// endpoint knows whether to clear the cookies. Computed here rather than at
    /// the endpoint because this handler already has to resolve which session is
    /// current, and asking twice invites the two answers to disagree.
    /// </summary>
    public async Task<bool> HandleAsync(
        RevokeSessionCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var current = await CurrentSessionIdAsync(
            command.RefreshToken, cancellationToken);

        if (command.SessionId is null)
        {
            await RevokeEveryOtherAsync(current, now, cancellationToken);

            return false;
        }

        var session = await _sessions.GetByIdAsync(
            SessionId.From(command.SessionId.Value), cancellationToken);

        // Not found and not yours are the same answer, deliberately.
        if (session is null || session.UserId != _currentUser.UserId)
            throw new NotFoundException(SessionErrors.NotFound);

        await _revoker.RevokeAsync(session, now, cancellationToken);

        return current is not null && session.Id == current;
    }

    /// <summary>
    /// "Sign out everywhere else" — the capability AUTH-009 deferred because it
    /// had no vocabulary for "else". It does now.
    /// </summary>
    private async Task RevokeEveryOtherAsync(
        SessionId? current, DateTime now, CancellationToken cancellationToken)
    {
        foreach (var session in await _sessions.GetActiveForUserAsync(
            _currentUser.UserId, cancellationToken))
        {
            if (current is not null && session.Id == current) continue;

            await _revoker.RevokeAsync(session, now, cancellationToken);
        }
    }

    private async Task<SessionId?> CurrentSessionIdAsync(
        string? refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;

        var token = await _refreshTokens.GetByHashAsync(
            _issuer.Hash(refreshToken), cancellationToken);

        return token?.SessionId;
    }
}
