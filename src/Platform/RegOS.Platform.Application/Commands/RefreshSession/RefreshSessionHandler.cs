using RegOS.Platform.Application.Authentication;
using RegOS.Platform.Application.Commands.Login;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.RefreshToken;
using RegOS.Platform.Domain.Aggregates.Session;
using RegOS.Platform.Domain.Aggregates.Tenant;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Exceptions;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Application.Commands.RefreshSession;

/// <summary>
/// Exchanges a refresh token for a new session, rotating the token in the
/// process.
///
/// Every failure raises <see cref="AuthenticationFailedException"/> with the
/// same message as sign-in, for the same reason (ADR-022): an unknown token, an
/// expired one, a revoked one and a token belonging to a deactivated user must
/// be indistinguishable to whoever presented it.
/// </summary>
public sealed class RefreshSessionHandler
{
    private readonly SessionFactory _sessions;
    private readonly SessionRevoker _revoker;
    private readonly IRefreshTokenIssuer _issuer;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ISessionRepository _sessionStore;
    private readonly IUserRepository _users;
    private readonly ITenantRepository _tenants;

    public RefreshSessionHandler(
        SessionFactory sessions,
        SessionRevoker revoker,
        IRefreshTokenIssuer issuer,
        IRefreshTokenRepository refreshTokens,
        ISessionRepository sessionStore,
        IUserRepository users,
        ITenantRepository tenants)
    {
        _sessions = sessions;
        _revoker = revoker;
        _issuer = issuer;
        _refreshTokens = refreshTokens;
        _sessionStore = sessionStore;
        _users = users;
        _tenants = tenants;
    }

    public async Task<AuthenticatedSession> HandleAsync(
        RefreshSessionCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            throw new AuthenticationFailedException(
                AuthenticationErrors.InvalidCredentials);
        }

        var presented = await _refreshTokens.GetByHashAsync(
            _issuer.Hash(command.RefreshToken), cancellationToken);

        if (presented is null)
        {
            throw new AuthenticationFailedException(
                AuthenticationErrors.InvalidCredentials);
        }

        var now = DateTime.UtcNow;

        if (!presented.IsActiveAt(now))
        {
            // A token that exists but is no longer active was either already
            // rotated or explicitly revoked. Presenting one means the client is
            // out of step or a stolen token is being replayed, and the two are
            // indistinguishable from here — so the whole session ends rather
            // than only this token being refused.
            //
            // Sessions only, not the full ADR-028 revocation: no credential was
            // replaced here, so outstanding reset grants are none of this
            // handler's business.
            await _revoker.RevokeEveryFor(
                presented.UserId, now, cancellationToken);

            throw new AuthenticationFailedException(
                AuthenticationErrors.InvalidCredentials);
        }

        var user = await _users.GetByIdAsync(presented.UserId, cancellationToken);

        // Checked on every refresh, not only at sign-in. Otherwise deactivating
        // someone would leave them working for as long as they kept refreshing.
        if (user is null || user.Status != UserStatus.Active)
        {
            throw new AuthenticationFailedException(
                AuthenticationErrors.InvalidCredentials);
        }

        // The tenant too: retiring a tenant ends its users' sessions at their
        // next refresh — at most the access token's fifteen minutes — without
        // touching any user record. Platform users have no tenant to check.
        if (user.TenantId is not null)
        {
            var tenant = await _tenants.GetByIdAsync(
                user.TenantId, cancellationToken);

            if (tenant is null || tenant.Status != TenantStatus.Active)
            {
                throw new AuthenticationFailedException(
                    AuthenticationErrors.InvalidCredentials);
            }
        }

        // The session the presented token belongs to. Revoked from the sessions
        // page means revoked here too, even while the token itself looks fine.
        var session = await _sessionStore.GetByIdAsync(
            presented.SessionId, cancellationToken);

        if (session is null || !session.IsActiveAt(now))
        {
            throw new AuthenticationFailedException(
                AuthenticationErrors.InvalidCredentials);
        }

        var (tokens, replacement) = _sessions.Continue(user, session, now);

        // Continue moved LastUsedOn and the expiry forward on the aggregate;
        // this is what makes the sessions page show "just now" rather than the
        // time the user originally signed in.
        await _sessionStore.UpdateAsync(session, cancellationToken);

        presented.RotateTo(replacement.Id, now);

        // One unit of work: the old token must die exactly when the new one is
        // born, or a crash between them leaves the user with two live tokens or
        // none at all.
        await _refreshTokens.RotateAsync(presented, replacement, cancellationToken);

        return tokens;
    }
}
