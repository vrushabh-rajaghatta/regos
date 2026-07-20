using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Commands.UpdateUserProfile;

/// <summary>
/// Updates a user's profile - name and email only. Status, roles, permissions
/// and organization membership are separate capabilities.
/// <paramref name="OrganizationId"/> scopes the update for tenant isolation and
/// is optional only because there is no authenticated tenant context yet.
/// </summary>
public sealed record UpdateUserProfileCommand(
    UserId UserId,
    string FirstName,
    string LastName,
    string Email,
    OrganizationId? OrganizationId = null);
