namespace RegOS.Api.Endpoints.Platform;

/// <summary>
/// The organization is deliberately absent: a user is invited into the caller's
/// own tenant, which arrives as a claim in their access token, not in the body
/// — so inviting into someone else's organization cannot be expressed.
/// </summary>
public sealed record InviteUserRequest(
    string FirstName,
    string LastName,
    string Email);
