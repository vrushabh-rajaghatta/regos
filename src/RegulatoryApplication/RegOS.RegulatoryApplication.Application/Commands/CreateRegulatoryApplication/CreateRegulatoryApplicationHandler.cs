using RegOS.RegulatoryApplication.Application.Services;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Application.Commands.CreateRegulatoryApplication;

public sealed class CreateRegulatoryApplicationHandler
{
    private readonly IRegulatoryApplicationCreationPolicy _creationPolicy;
    private readonly IRegulatoryApplicationRepository _repository;

    public CreateRegulatoryApplicationHandler(
        IRegulatoryApplicationCreationPolicy creationPolicy,
        IRegulatoryApplicationRepository repository)
    {
        _creationPolicy = creationPolicy;
        _repository = repository;
    }

    public async Task<CreateRegulatoryApplicationResult> HandleAsync(
        CreateRegulatoryApplicationCommand command,
        CancellationToken cancellationToken)
    {
        await _creationPolicy.EnsureCanCreateAsync(
            command.ProductId,
            command.CountryId,
            command.AuthorityId,
            command.ApplicantOrganizationId,
            cancellationToken);

        var regulatoryApplication = RegulatoryApplicationAggregate.Create(
            command.ProductId,
            command.CountryId,
            command.AuthorityId,
            command.ApplicantOrganizationId,
            command.Name);

        await _repository.AddAsync(
            regulatoryApplication,
            cancellationToken);

        return new CreateRegulatoryApplicationResult(
            regulatoryApplication.Id);
    }
}
