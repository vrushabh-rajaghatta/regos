namespace RegOS.Api.Endpoints.Platform;

public sealed record InviteUserRequest(
    Guid OrganizationId,
    string FirstName,
    string LastName,
    string Email);
