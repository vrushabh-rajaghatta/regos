using RegOS.Platform.Domain.Aggregates.Tenant;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Application.Commands.ActivateTenant;

public sealed class ActivateTenantHandler
{
    private readonly ITenantRepository _repository;

    public ActivateTenantHandler(ITenantRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        ActivateTenantCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(
            command.TenantId, cancellationToken);

        if (tenant is null)
            throw new NotFoundException(PlatformErrors.TenantNotFound);

        // The aggregate decides whether the transition is legal; activating
        // an already-active tenant raises from there.
        tenant.Activate();

        await _repository.UpdateAsync(tenant, cancellationToken);
    }
}
