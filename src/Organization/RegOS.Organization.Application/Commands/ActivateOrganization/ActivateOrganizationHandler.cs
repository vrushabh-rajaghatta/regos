using RegOS.Organization.Application.Persistence;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Organization.Application.Commands.ActivateOrganization;

public sealed class ActivateOrganizationHandler
{
    private readonly IOrganizationRepository _repository;
    private readonly ITenantContext _tenantContext;

    public ActivateOrganizationHandler(
        IOrganizationRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task HandleAsync(
        ActivateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        var organization = await _repository.GetByIdAsync(
            command.Id,
            cancellationToken);

        // Interim ownership rule until organizations belong to a tenant
        // (ADR-030): see UpdateOrganizationHandler.
        if (organization is null
            || organization.Id.Value != _tenantContext.TenantId.Value)
        {
            throw new NotFoundException(OrganizationErrors.NotFound);
        }

        // The aggregate decides whether the transition is legal; activating an
        // already-active organization raises from there.
        organization.Activate();

        await _repository.UpdateAsync(organization, cancellationToken);
    }
}
