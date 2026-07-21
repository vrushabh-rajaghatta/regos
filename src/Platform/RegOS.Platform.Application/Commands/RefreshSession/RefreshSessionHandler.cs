using RegOS.Platform.Application.Authentication;
using RegOS.Platform.Application.Commands.Login;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.RefreshToken;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Exceptions;

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
    private readonly IRefreshTokenIssuer _issuer;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUserRepository _users;

    public RefreshSessionHandler(
        SessionFactory sessions,
        IRefreshTokenIssuer issuer,
        IRefreshTokenRepository refreshTokens,
        IUserRepository users)
    {
        _sessions = sessions;
        _issuer = issuer;
        _refreshTokens = refreshTokens;
        _users = users;
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
            await RevokeEverySessionForAsync(
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

        var (session, replacement) = _sessions.Create(user, now);

        presented.RotateTo(replacement.Id, now);

        // One unit of work: the old token must die exactly when the new one is
        // born, or a crash between them leaves the user with two live tokens or
        // none at all.
        await _refreshTokens.RotateAsync(presented, replacement, cancellationToken);

        return session;
    }

    private async Task RevokeEverySessionForAsync(
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
