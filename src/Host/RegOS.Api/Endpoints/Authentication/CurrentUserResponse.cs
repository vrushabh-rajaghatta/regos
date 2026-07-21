namespace RegOS.Api.Endpoints.Authentication;

/// <summary>
/// Exactly what the token carries, and nothing resolved from the database.
/// This endpoint answers "is my token still good, and who does it say I am",
/// so reading a user row to enrich it would answer a different question more
/// slowly.
/// </summary>
/// <remarks>
/// TenantId is null for a platform user, whose token carries no tenant claim
/// (ADR-030). Faithful to the token: absent there, null here.
/// </remarks>
public sealed record CurrentUserResponse(
    Guid UserId,
    Guid? TenantId,
    string Email,
    string Role);
