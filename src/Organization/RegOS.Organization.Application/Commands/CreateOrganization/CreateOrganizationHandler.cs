using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Abstractions;

using OrganizationAggregate =
    RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Organization.Application.Commands.CreateOrganization;

public sealed class CreateOrganizationHandler
{
    private readonly IOrganizationRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateOrganizationHandler(
        IOrganizationRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<OrganizationId> HandleAsync(
        CreateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        // Into the caller's own registry, ambient like every other tenant
        // scope (ADR-013). Creating an organization used to be the one
        // unscoped operation — it was what brought a tenant into existence —
        // but ADR-030 gave tenants their own aggregate and ADR-032 made the
        // registry tenant-owned, so an organization created here is visible
        // to this tenant and no other.
        //
        // The aggregate owns the invariants; the handler never reimplements
        // them. A missing legal name raises DomainException (400) from Create.
        var organization = OrganizationAggregate.Create(
            _tenantContext.TenantId,
            command.LegalName!,
            command.Type);

        await _repository.AddAsync(organization, cancellationToken);

        return organization.Id;
    }
}
