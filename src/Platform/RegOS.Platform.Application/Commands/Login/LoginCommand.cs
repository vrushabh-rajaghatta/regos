namespace RegOS.Platform.Application.Commands.Login;

public sealed record LoginCommand(string? Email, string? Password);

/// <param name="AccessToken">The bearer token to send on subsequent requests.</param>
/// <param name="ExpiresAt">
/// When the token stops being accepted, so the client can refresh ahead of
/// expiry instead of discovering it through a failed request.
/// </param>
public sealed record LoginResult(string AccessToken, DateTime ExpiresAt);
