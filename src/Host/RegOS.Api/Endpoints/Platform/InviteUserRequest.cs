namespace RegOS.Api.Endpoints.Platform;

/// <summary>
/// The organization is deliberately absent: a user is invited into the caller's
/// own tenant, which arrives in the X-Tenant-Id header, not the body.
/// </summary>
public sealed record InviteUserRequest(
    string FirstName,
    string LastName,
    string Email);
