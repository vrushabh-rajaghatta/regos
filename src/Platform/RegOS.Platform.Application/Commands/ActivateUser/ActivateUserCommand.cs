using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Platform.Application.Commands.ActivateUser;

/// <summary>
/// Grants an invited or inactive user access to RegOS. Activation carries no
/// payload - it is a business decision, not a property update.
/// <paramref name="OrganizationId"/> scopes the action for tenant isolation and
/// is optional only because there is no authenticated tenant context yet.
/// </summary>
public sealed record ActivateUserCommand(
    UserId UserId,
    OrganizationId? OrganizationId = null);
