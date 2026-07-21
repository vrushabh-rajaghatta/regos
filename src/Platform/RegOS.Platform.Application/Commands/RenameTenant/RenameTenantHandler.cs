using RegOS.Platform.Domain.Aggregates.Tenant;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Application.Commands.RenameTenant;

/// <summary>
/// Load, invoke the aggregate behaviour, persist. Deliberately does NOT
/// rename the mirror organization: the tenant's name is an account label,
/// the organization's legal name is a regulatory fact, and after provisioning
/// they diverge freely — a rebrand of the account is not a change of legal
/// entity (ADR-032).
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
