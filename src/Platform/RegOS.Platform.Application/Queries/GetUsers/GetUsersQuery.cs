using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Queries.GetUsers;

/// <summary>
/// Browses the user directory. <paramref name="OrganizationId"/> is optional
/// only because there is no authenticated user context yet — once sign-in
/// exists the organization comes from the caller rather than the query string.
/// <paramref name="Search"/> matches first name, last name or email.
/// </summary>
public sealed record GetUsersQuery(
    OrganizationId? OrganizationId = null,
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
