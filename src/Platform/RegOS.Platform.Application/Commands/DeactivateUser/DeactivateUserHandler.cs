using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Platform.Application.Common;
using RegOS.Platform.Domain.Aggregates.User;
using RegOS.SharedKernel.Abstractions;

namespace RegOS.Platform.Application.Commands.DeactivateUser;

/// <summary>
/// Load, invoke the aggregate behaviour, persist. Deactivation preserves the
/// profile: this is a revocation of access, not a deletion. The lifecycle rule
/// (including idempotency) belongs to the aggregate.
/// </summary>
public sealed class DeactivateUserHandler
{
    private readonly IUserRepository _repository;
    private readonly ITenantContext _tenantContext;

    public DeactivateUserHandler(
        IUserRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task HandleAsync(
        DeactivateUserCommand command,
        CancellationToken cancellationToken)
    {
        // The tenant becomes a domain identifier here, at the application
        // boundary - the shared kernel deals only in guids.
        var organizationId = new OrganizationId(_tenantContext.TenantId);

        var user = await _repository.GetRequiredAsync(
            command.UserId, organizationId, cancellationToken);

        user.Deactivate();

        await _repository.UpdateAsync(user, cancellationToken);
    }
}
