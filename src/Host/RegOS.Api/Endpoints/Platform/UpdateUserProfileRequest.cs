namespace RegOS.Api.Endpoints.Platform;

public sealed record UpdateUserProfileRequest(
    string FirstName,
    string LastName,
    string Email);
