using RegOS.Organization.Application.Persistence;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Organization.Application.Commands.DeactivateOrganization;

public sealed class DeactivateOrganizationHandler
{
    private readonly IOrganizationRepository _repository;
    private readonly ITenantContext _tenantContext;

    public DeactivateOrganizationHandler(
        IOrganizationRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task HandleAsync(
        DeactivateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        var organization = await _repository.GetByIdAsync(
            command.Id,
            cancellationToken);

        // Interim ownership rule until organizations belong to a tenant
        // (ADR-030): see UpdateOrganizationHandler. Deactivating another
        // customer's organization would be a cross-tenant denial of service.
        if (organization is null
            || organization.Id.Value != _tenantContext.TenantId.Value)
        {
            throw new NotFoundException(OrganizationErrors.NotFound);
        }

        // The aggregate decides whether the transition is legal; deactivating
        // an already-inactive organization raises from there.
        organization.Deactivate();

        await _repository.UpdateAsync(organization, cancellationToken);
    }
}
