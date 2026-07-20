using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Queries.GetUsers;

/// <summary>
/// A row in the user directory — a read-only projection optimized for browsing,
/// deliberately NOT the User aggregate. Exposes only what the directory screen
/// needs: no OrganizationId, no navigation properties, no internal state.
/// </summary>
public sealed record UserListItem(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserStatus Status,
    DateTime CreatedOn);
