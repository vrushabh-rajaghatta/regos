using RegOS.Platform.Domain.Aggregates.Tenant;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Platform.Application.Commands.DeactivateTenant;

/// <summary>
/// Retires the tenant. Its users are untouched as records, but "no one signs
/// in" is enforced where sign-in lives: login and refresh both check the
/// tenant's status, so existing sessions end at the next refresh — at most
/// fifteen minutes — and new ones cannot start.
/// </summary>
public sealed class DeactivateTenantHandler
{
    private readonly ITenantRepository _repository;

    public DeactivateTenantHandler(ITenantRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        DeactivateTenantCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(
            command.TenantId, cancellationToken);

        if (tenant is null)
            throw new NotFoundException(PlatformErrors.TenantNotFound);

        // The aggregate decides whether the transition is legal.
        tenant.Deactivate();

        await _repository.UpdateAsync(tenant, cancellationToken);
    }
}
