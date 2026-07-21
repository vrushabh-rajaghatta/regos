using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Queries.GetTenantUsers;

public sealed record TenantUserListItem(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserStatus Status,
    UserRole Role);
