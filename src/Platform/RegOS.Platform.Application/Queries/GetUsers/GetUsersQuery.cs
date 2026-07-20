using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Queries.GetUsers;

/// <summary>
/// Browses the user directory for the caller's tenant. The tenant is ambient,
/// so it is not a parameter here and cannot be widened by omitting it.
/// <paramref name="Search"/> matches first name, last name or email.
/// </summary>
public sealed record GetUsersQuery(
    string? Search = null,
    UserStatus? Status = null,
    int Page = GetUsersQuery.DefaultPage,
    int PageSize = GetUsersQuery.DefaultPageSize)
{
    public const int DefaultPage = 1;

    public const int DefaultPageSize = 20;

    /// <summary>Unbounded reads are never allowed.</summary>
    public const int MaxPageSize = 100;
}
