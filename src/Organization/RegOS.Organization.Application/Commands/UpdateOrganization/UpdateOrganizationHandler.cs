using RegOS.Organization.Application.Persistence;
using RegOS.Organization.Domain.Aggregates.Organization;
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

    public UpdateOrganizationHandler(IOrganizationRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        UpdateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        var organization = await _repository.GetByIdAsync(
            command.Id,
            cancellationToken);

        // Addressed by the route and absent: 404, not 400 (ADR-009).
        if (organization is null)
            throw new NotFoundException(OrganizationErrors.NotFound);

        // The aggregate owns the invariants and the intent of each change; the
        // handler never reimplements them. Submitting unchanged values is a
        // no-op — EF issues no UPDATE when nothing differs, and there is no
        // version to increment.
        organization.Rename(command.LegalName);
        organization.Reclassify(command.Type);

        await _repository.UpdateAsync(organization, cancellationToken);
    }
}
