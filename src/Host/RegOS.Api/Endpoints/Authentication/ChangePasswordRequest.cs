namespace RegOS.Api.Endpoints.Authentication;

public sealed record ChangePasswordRequest(
    string? CurrentPassword,
    string? NewPassword);
