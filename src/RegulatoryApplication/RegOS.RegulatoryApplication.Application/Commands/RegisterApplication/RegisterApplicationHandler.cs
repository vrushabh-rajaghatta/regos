using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Application.Commands.RegisterApplication;

public sealed class RegisterApplicationHandler
{
    private readonly IRegulatoryApplicationRepository _repository;

    public RegisterApplicationHandler(
        IRegulatoryApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<RegisterApplicationResult> HandleAsync(
        RegisterApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var application = RegulatoryApplicationAggregate.Create(
            command.ProductId,
            command.AuthorityId,
            command.CountryId,
            command.ApplicantOrganizationId,
            command.Name);

        await _repository.AddAsync(
            application,
            cancellationToken);

        return new RegisterApplicationResult(
            application.Id);
    }
}