using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Queries.GetUserById;

/// <summary>
/// Reads a single user. <paramref name="OrganizationId"/> scopes the lookup for
/// tenant isolation: when supplied, a user belonging to a different
/// organization is reported as not found. It is optional only because there is
/// no authenticated tenant context yet - once sign-in exists the organization
/// comes from the caller rather than the query string.
/// </summary>
public sealed record GetUserByIdQuery(
    UserId UserId,
    OrganizationId? OrganizationId = null);
