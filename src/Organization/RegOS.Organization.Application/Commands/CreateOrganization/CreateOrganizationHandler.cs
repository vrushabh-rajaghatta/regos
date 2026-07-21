using RegOS.Organization.Application.Persistence;
using RegOS.Organization.Domain.Aggregates.Organization;

using OrganizationAggregate =
    RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Organization.Application.Commands.CreateOrganization;

public sealed class CreateOrganizationHandler
{
    private readonly IOrganizationRepository _repository;

    public CreateOrganizationHandler(IOrganizationRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrganizationId> HandleAsync(
        CreateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        // The aggregate owns the invariants; the handler never reimplements
        // them. A missing legal name raises DomainException (400) from Create.
        var organization = OrganizationAggregate.Create(
            command.LegalName!,
            command.Type);

        await _repository.AddAsync(organization, cancellationToken);

        return organization.Id;
    }
}
