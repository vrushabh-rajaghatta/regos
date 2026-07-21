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
        var user = await _repository.GetRequiredAsync(
            command.UserId, _tenantContext.TenantId, cancellationToken);

        user.Deactivate();

        await _repository.UpdateAsync(user, cancellationToken);
    }
}
