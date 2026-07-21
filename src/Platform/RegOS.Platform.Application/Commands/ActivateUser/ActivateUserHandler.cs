using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Common;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Application.Commands.ActivateUser;

/// <summary>
/// Restores a deactivated account, and nothing else.
/// </summary>
/// <remarks>
/// This is one of two paths to <c>Active</c>, and the narrower one. Acceptance
/// is the other, and is how an <c>Invited</c> user gets there — activating one
/// from here was the only way to reach <c>Active</c> without ever setting a
/// password, so ADR-027 removed it.
///
/// The restriction lives here rather than on the aggregate deliberately.
/// <c>User.Activate()</c> still means "make this user active" and both paths
/// invoke it; what differs is who may ask, and from what state. The aggregate
/// owns what activation means; the handler owns who is allowed to request it.
/// </remarks>
public sealed class ActivateUserHandler
{
    private readonly IUserRepository _repository;
    private readonly ITenantContext _tenantContext;

    public ActivateUserHandler(
        IUserRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task HandleAsync(
        ActivateUserCommand command,
        CancellationToken cancellationToken)
    {
        // The tenant becomes a domain identifier here, at the application
        // boundary - the shared kernel deals only in guids.
        var organizationId = new OrganizationId(_tenantContext.TenantId);

        var user = await _repository.GetRequiredAsync(
            command.UserId, organizationId, cancellationToken);

        if (user.Status != UserStatus.Inactive)
        {
            throw new BusinessRuleViolationException(
                UserErrors.OnlyInactiveUsersCanBeActivated);
        }

        user.Activate();

        await _repository.UpdateAsync(user, cancellationToken);
    }
}
