using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.RefreshToken;

using RefreshTokenAggregate =
    RegOS.Platform.Domain.Aggregates.RefreshToken.RefreshToken;
using SessionAggregate = RegOS.Platform.Domain.Aggregates.Session.Session;
using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Authentication;

/// <summary>
/// Begins a session, and continues one.
/// </summary>
/// <remarks>
/// One class rather than the same sequence written in both <c>LoginHandler</c>
/// and <c>RefreshSessionHandler</c>. This is not the duplication ADR-018 tells
/// us to tolerate twice: two independent implementations of "start a session"
/// is how a refresh path quietly acquires a longer lifetime, or stops hashing,
/// without anyone noticing.
///
/// The two methods are the AUTH-010 distinction made explicit. Signing in
/// starts a session; refreshing continues the one that already exists. Before,
/// both simply minted a token and the difference was invisible.
///
/// Deliberately a concrete class with no interface — it hides no infrastructure
/// and exists to be used, not substituted.
/// </remarks>
public sealed class SessionFactory
{
    private readonly IAccessTokenIssuer _accessTokens;
    private readonly IRefreshTokenIssuer _refreshTokens;

    public SessionFactory(
        IAccessTokenIssuer accessTokens,
        IRefreshTokenIssuer refreshTokens)
    {
        _accessTokens = accessTokens;
        _refreshTokens = refreshTokens;
    }

    /// <summary>Signing in: a new session, and the first token carrying it.</summary>
    public (AuthenticatedSession Tokens,
        SessionAggregate Session,
        RefreshTokenAggregate Record) Start(
            UserAggregate user,
            string? userAgent,
            string? ipAddress,
            DateTime now)
    {
        var access = _accessTokens.Issue(user);
        var refresh = _refreshTokens.Issue(now);

        var session = SessionAggregate.Start(
            user.Id, userAgent, ipAddress, refresh.ExpiresAt, now);

        var record = RefreshTokenAggregate.Issue(
            user.Id, session.Id, refresh.Hash, refresh.ExpiresAt, now);

        return (Tokens(access, refresh), session, record);
    }

    /// <summary>
    /// Refreshing: the same session, a new token. The session's identity is
    /// what the user sees, so it must not change here.
    /// </summary>
    public (AuthenticatedSession Tokens, RefreshTokenAggregate Record) Continue(
        UserAggregate user,
        SessionAggregate session,
        DateTime now)
    {
        var access = _accessTokens.Issue(user);
        var refresh = _refreshTokens.Issue(now);

        session.Refreshed(refresh.ExpiresAt, now);

        var record = RefreshTokenAggregate.Issue(
            user.Id, session.Id, refresh.Hash, refresh.ExpiresAt, now);

        return (Tokens(access, refresh), record);
    }

    private static AuthenticatedSession Tokens(
        AccessToken access, IssuedRefreshToken refresh) =>
        new(access.Value,
            access.ExpiresAt,
            refresh.Value,
            refresh.ExpiresAt);
}
