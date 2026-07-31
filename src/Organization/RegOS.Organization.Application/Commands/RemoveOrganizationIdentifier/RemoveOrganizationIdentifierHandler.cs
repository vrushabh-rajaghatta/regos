using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Organization.Application.Commands.RemoveOrganizationIdentifier;

public sealed class RemoveOrganizationIdentifierHandler
{
    private readonly IOrganizationRepository _repository;

    public RemoveOrganizationIdentifierHandler(
        IOrganizationRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        RemoveOrganizationIdentifierCommand command,
        CancellationToken cancellationToken)
    {
        var organization = await _repository.GetByIdAsync(
            command.OrganizationId,
            cancellationToken);

        if (organization is null)
            throw new NotFoundException(OrganizationErrors.NotFound);

        // An identifier belonging to another organization is not found here
        // either: the aggregate only searches its own.
        organization.RemoveIdentifier(command.IdentifierId);

        await _repository.UpdateAsync(organization, cancellationToken);
    }
}
