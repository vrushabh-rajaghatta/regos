using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.RefreshToken;
using RegOS.Platform.Domain.Aggregates.Session;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Application.Queries.GetSessions;

/// <summary>
/// The signed-in user's own sessions, newest activity first.
/// </summary>
/// <remarks>
/// Reads through the repository rather than a projection because the list is
/// small, per-user, and already exactly the aggregate's shape — the read-model
/// machinery (ADR-006) would earn nothing here.
///
/// Scoped to <c>ICurrentUser</c> and not to a tenant: sessions belong to a
/// person, not an organization, and an administrator viewing someone else's
/// sessions is a different capability that nobody has asked for.
/// </remarks>
public sealed class GetSessionsHandler
{
    private readonly ICurrentUser _currentUser;
    private readonly IRefreshTokenIssuer _issuer;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ISessionRepository _sessions;

    public GetSessionsHandler(
        ICurrentUser currentUser,
        IRefreshTokenIssuer issuer,
        IRefreshTokenRepository refreshTokens,
        ISessionRepository sessions)
    {
        _currentUser = currentUser;
        _issuer = issuer;
        _refreshTokens = refreshTokens;
        _sessions = sessions;
    }

    public async Task<IReadOnlyList<SessionSummary>> HandleAsync(
        GetSessionsQuery query,
        CancellationToken cancellationToken)
    {
        var current = await CurrentSessionIdAsync(
            query.RefreshToken, cancellationToken);

        var sessions = await _sessions.GetActiveForUserAsync(
            _currentUser.UserId, cancellationToken);

        return sessions
            .Select(session => new SessionSummary(
                session.Id.Value,
                session.UserAgent,
                session.CreatedFromIp,
                session.CreatedOn,
                session.LastUsedOn,
                session.Id == current))
            .ToList();
    }

    /// <summary>
    /// Which session is asking, worked out from the refresh cookie rather than
    /// the access token. The access token says who you are; only the refresh
    /// token says which sign-in you are on, and the cookie is scoped to
    /// <c>/api/auth</c> so it does reach this endpoint.
    /// </summary>
    private async Task<SessionId?> CurrentSessionIdAsync(
        string? refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;

        var token = await _refreshTokens.GetByHashAsync(
            _issuer.Hash(refreshToken), cancellationToken);

        return token?.SessionId;
    }
}
