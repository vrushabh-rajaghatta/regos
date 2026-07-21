namespace RegOS.Api.Endpoints.Authentication;

public sealed record CompletePasswordResetRequest(string? Token, string? Password);
