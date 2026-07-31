using RegOS.Organization.Application.Services;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Organization.Application.Commands.AddOrganizationIdentifier;

public sealed class AddOrganizationIdentifierHandler
{
    private readonly IOrganizationIdentifierPolicy _policy;
    private readonly IOrganizationRepository _repository;

    public AddOrganizationIdentifierHandler(
        IOrganizationIdentifierPolicy policy,
        IOrganizationRepository repository)
    {
        _policy = policy;
        _repository = repository;
    }

    public async Task<OrganizationIdentifierId> HandleAsync(
        AddOrganizationIdentifierCommand command,
        CancellationToken cancellationToken)
    {
        // The cross-table rule first, matching CreateOrganizationSite.
        await _policy.EnsureSchemeExistsAsync(command.SchemeId, cancellationToken);

        var organization = await _repository.GetByIdAsync(
            command.OrganizationId,
            cancellationToken);

        // Absent or another tenant's — the query filter (ADR-032) makes both a
        // 404. See UpdateOrganizationHandler.
        if (organization is null)
            throw new NotFoundException(OrganizationErrors.NotFound);

        // One per scheme is the aggregate's rule; a second DUNS number would
        // mean one of them is wrong. It raises from there, not from here.
        var identifier = organization.AddIdentifier(
            command.SchemeId,
            command.Value);

        await _repository.UpdateAsync(organization, cancellationToken);

        return identifier.Id;
    }
}
