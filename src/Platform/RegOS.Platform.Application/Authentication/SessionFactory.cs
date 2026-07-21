using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.RefreshToken;

using RefreshTokenAggregate =
    RegOS.Platform.Domain.Aggregates.RefreshToken.RefreshToken;
using UserAggregate = RegOS.Platform.Domain.Aggregates.User.User;

namespace RegOS.Platform.Application.Authentication;

/// <summary>
/// Builds a session: an access token, a refresh token, and the stored record of
/// the latter.
/// </summary>
/// <remarks>
/// One class rather than the same sequence written in both
/// <c>LoginHandler</c> and <c>RefreshSessionHandler</c>. This is not the
/// duplication ADR-018 tells us to tolerate twice: two independent
/// implementations of "start a session" is how a refresh path quietly acquires
/// a longer lifetime, or stops hashing, without anyone noticing. There is one
/// way to begin a session and this is it.
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

    public (AuthenticatedSession Session, RefreshTokenAggregate Record) Create(
        UserAggregate user,
        DateTime now)
    {
        var access = _accessTokens.Issue(user);
        var refresh = _refreshTokens.Issue(now);

        var record = RefreshTokenAggregate.Issue(
            user.Id, refresh.Hash, refresh.ExpiresAt, now);

        return (
            new AuthenticatedSession(
                access.Value,
                access.ExpiresAt,
                refresh.Value,
                refresh.ExpiresAt),
            record);
    }
}
