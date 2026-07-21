namespace RegOS.Api.Endpoints.Authentication;

/// <summary>
/// Exactly what the token carries, and nothing resolved from the database.
/// This endpoint answers "is my token still good, and who does it say I am",
/// so reading a user row to enrich it would answer a different question more
/// slowly.
/// </summary>
public sealed record CurrentUserResponse(
    Guid UserId,
    Guid TenantId,
    string Email);
