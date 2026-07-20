using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Commands.DeactivateUser;

/// <summary>
/// Revokes a user's access without deleting them - their account and history
/// are preserved, and they can be reactivated later. Carries no payload.
/// <paramref name="OrganizationId"/> scopes the action for tenant isolation and
/// is optional only because there is no authenticated tenant context yet.
/// </summary>
public sealed record DeactivateUserCommand(
    UserId UserId,
    OrganizationId? OrganizationId = null);
