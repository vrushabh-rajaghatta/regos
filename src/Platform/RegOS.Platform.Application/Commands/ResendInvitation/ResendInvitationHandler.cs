using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Common;
using RegOS.Platform.Application.Invitations;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Application.Commands.ResendInvitation;

/// <summary>
/// Issues a fresh invitation and retires the previous one.
/// </summary>
/// <remarks>
/// Tenant-scoped, unlike acceptance: an administrator is asking, on behalf of
/// their own organization, so the same rules as every other user-administration
/// command apply.
///
/// Also the remediation path for users invited before invitations carried
/// tokens — they have no invitation at all, and this gives them one.
/// </remarks>
public sealed class ResendInvitationHandler
{
    private readonly InvitationIssuer _invitations;
    private readonly IUserRepository _repository;
    private readonly ITenantContext _tenantContext;

    public ResendInvitationHandler(
        InvitationIssuer invitations,
        IUserRepository repository,
        ITenantContext tenantContext)
    {
        _invitations = invitations;
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task HandleAsync(
        ResendInvitationCommand command,
        CancellationToken cancellationToken)
    {
        var organizationId = new OrganizationId(_tenantContext.TenantId);

        var user = await _repository.GetRequiredAsync(
            command.UserId, organizationId, cancellationToken);

        // Only someone still waiting to accept can be re-invited. Resending to
        // an active user would hand out a token that acceptance would refuse,
        // and resending to a deactivated one would undo the deactivation's
        // intent.
        if (user.Status != UserStatus.Invited)
        {
            throw new BusinessRuleViolationException(
                UserErrors.OnlyInvitedUsersCanBeReinvited);
        }

        await _invitations.IssueAsync(user, DateTime.UtcNow, cancellationToken);
    }
}
