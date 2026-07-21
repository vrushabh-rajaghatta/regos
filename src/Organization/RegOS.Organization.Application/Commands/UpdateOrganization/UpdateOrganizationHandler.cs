using RegOS.Organization.Application.Persistence;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Organization.Application.Commands.UpdateOrganization;

/// <summary>
/// The write-side pattern, with no special cases: load the aggregate through
/// the repository, invoke its behaviour, persist. No DbContext, no projection,
/// no unit of work.
/// </summary>
public sealed class UpdateOrganizationHandler
{
    private readonly IOrganizationRepository _repository;
    private readonly ITenantContext _tenantContext;

    public UpdateOrganizationHandler(
        IOrganizationRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task HandleAsync(
        UpdateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        var organization = await _repository.GetByIdAsync(
            command.Id,
            cancellationToken);

        // Interim ownership rule until organizations belong to a tenant
        // (ADR-030): the only organization a caller may mutate is the one that
        // shares their tenant's id — its pre-split alter ego. Without this,
        // any signed-in user could rename any customer's organization by guid.
        // Reported as not found, never forbidden, like every other tenant
        // mismatch. Reads stay global by design: the directory is shared.
        if (organization is null
            || organization.Id.Value != _tenantContext.TenantId.Value)
        {
            throw new NotFoundException(OrganizationErrors.NotFound);
        }

        // The aggregate owns the invariants and the intent of each change; the
        // handler never reimplements them. Submitting unchanged values is a
        // no-op — EF issues no UPDATE when nothing differs, and there is no
        // version to increment.
        organization.Rename(command.LegalName);
        organization.Reclassify(command.Type);

        await _repository.UpdateAsync(organization, cancellationToken);
    }
}
