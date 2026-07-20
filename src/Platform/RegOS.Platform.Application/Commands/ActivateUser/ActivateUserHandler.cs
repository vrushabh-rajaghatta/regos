using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Common;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Abstractions;

namespace RegOS.Platform.Application.Commands.ActivateUser;

/// <summary>
/// Load, invoke the aggregate behaviour, persist. There is no policy here: the
/// lifecycle rule (including idempotency) belongs to the aggregate, and nothing
/// about activation needs to ask the outside world.
/// </summary>
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

        user.Activate();

        await _repository.UpdateAsync(user, cancellationToken);
    }
}
