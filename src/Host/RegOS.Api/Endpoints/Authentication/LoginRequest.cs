namespace RegOS.Api.Endpoints.Authentication;

public sealed record LoginRequest(string? Email, string? Password);
