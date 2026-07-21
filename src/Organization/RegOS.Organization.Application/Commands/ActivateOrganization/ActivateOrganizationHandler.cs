using RegOS.Organization.Application.Persistence;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Organization.Application.Commands.ActivateOrganization;

public sealed class ActivateOrganizationHandler
{
    private readonly IOrganizationRepository _repository;

    public ActivateOrganizationHandler(IOrganizationRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        ActivateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        var organization = await _repository.GetByIdAsync(
            command.Id,
            cancellationToken);

        // Addressed by the route and absent: 404, not 400 (ADR-009).
        if (organization is null)
            throw new NotFoundException(OrganizationErrors.NotFound);

        // The aggregate decides whether the transition is legal; activating an
        // already-active organization raises from there.
        organization.Activate();

        await _repository.UpdateAsync(organization, cancellationToken);
    }
}
