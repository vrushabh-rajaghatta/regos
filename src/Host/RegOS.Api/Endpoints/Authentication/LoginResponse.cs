namespace RegOS.Api.Endpoints.Authentication;

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAt);
