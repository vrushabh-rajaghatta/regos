using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Queries.GetUserById;

/// <summary>
/// Read-only projection of a single user for the details screen. Exposes only
/// what the screen shows today - no roles, permissions or other anticipated
/// fields.
/// </summary>
public sealed record UserDetails(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserStatus Status,
    DateTime CreatedOn);
