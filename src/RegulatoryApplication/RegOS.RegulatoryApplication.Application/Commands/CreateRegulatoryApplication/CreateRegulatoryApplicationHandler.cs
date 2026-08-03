using RegOS.RegulatoryApplication.Application.Services;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Abstractions;
using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Application.Commands.CreateRegulatoryApplication;

public sealed class CreateRegulatoryApplicationHandler
{
    private readonly IRegulatoryApplicationCreationPolicy _creationPolicy;
    private readonly IRegulatoryApplicationRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateRegulatoryApplicationHandler(
        IRegulatoryApplicationCreationPolicy creationPolicy,
        IRegulatoryApplicationRepository repository,
        ITenantContext tenantContext)
    {
        _creationPolicy = creationPolicy;
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<CreateRegulatoryApplicationResult> HandleAsync(
        CreateRegulatoryApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var applicationType = await _creationPolicy.EnsureCanCreateAsync(
            command.GlobalProductId,
            command.CountryId,
            command.AuthorityId,
            command.ApplicationTypeId,
            command.ApplicantOrganizationId,
            cancellationToken);

        // The owner is ambient (who is asking); the applicant stays an
        // explicit command property (who the filing is on behalf of). The
        // first regulatory record to carry both, and the distinction is the
        // whole point of ADR-030.
        var regulatoryApplication = RegulatoryApplicationAggregate.Create(
            _tenantContext.TenantId,
            command.GlobalProductId,
            command.CountryId,
            command.AuthorityId,
            applicationType,
            command.ApplicantOrganizationId,
            command.Name);

        await _repository.AddAsync(
            regulatoryApplication,
            cancellationToken);

        return new CreateRegulatoryApplicationResult(
            regulatoryApplication.Id);
    }
}
