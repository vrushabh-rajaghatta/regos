using RegOS.Platform.Domain.Aggregates.Tenant;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Application.Commands.RenameTenant;

/// <summary>
/// Load, invoke the aggregate behaviour, persist. The tenant's name is an
/// account label and reaches nothing else: provisioning creates no
/// organization to keep in step (ADR-060), and a tenant's own company — once
/// its administrator records one — carries a legal name renamed through the
/// organization registry, on its own terms.
/// </summary>
public sealed class RenameTenantHandler
{
    private readonly ITenantRepository _repository;

    public RenameTenantHandler(ITenantRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        RenameTenantCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(
            command.TenantId, cancellationToken);

        // Addressed by the route and absent: 404, not 400 (ADR-009).
        if (tenant is null)
            throw new NotFoundException(PlatformErrors.TenantNotFound);

        tenant.Rename(command.Name);

        await _repository.UpdateAsync(tenant, cancellationToken);
    }
}
