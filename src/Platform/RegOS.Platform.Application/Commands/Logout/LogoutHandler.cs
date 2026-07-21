using RegOS.Platform.Application.Services;
using RegOS.Platform.Domain.Aggregates.RefreshToken;

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
    private readonly IRefreshTokenIssuer _issuer;
    private readonly IRefreshTokenRepository _refreshTokens;

    public LogoutHandler(
        IRefreshTokenIssuer issuer,
        IRefreshTokenRepository refreshTokens)
    {
        _issuer = issuer;
        _refreshTokens = refreshTokens;
    }

    public async Task HandleAsync(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken)) return;

        var token = await _refreshTokens.GetByHashAsync(
            _issuer.Hash(command.RefreshToken), cancellationToken);

        if (token is null) return;

        token.Revoke(DateTime.UtcNow);

        await _refreshTokens.UpdateAsync(token, cancellationToken);
    }
}
