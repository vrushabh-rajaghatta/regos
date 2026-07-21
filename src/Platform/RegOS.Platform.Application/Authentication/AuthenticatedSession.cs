namespace RegOS.Platform.Application.Authentication;

/// <param name="AccessToken">Short-lived; proves identity on every request.</param>
/// <param name="AccessTokenExpiresAt">When the access token stops being accepted.</param>
/// <param name="RefreshToken">
/// Long-lived; buys a new access token. Returned exactly once — only its hash
/// is stored, so it can never be produced again.
/// </param>
/// <param name="RefreshTokenExpiresAt">When the session ends outright.</param>
public sealed record AuthenticatedSession(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);
