using RegOS.RegulatoryApplication.Domain.Aggregates.Application;
using ApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.Application.Application;

namespace RegOS.RegulatoryApplication.Application.Commands.RegisterApplication;

public sealed class RegisterApplicationHandler
{
    private readonly IApplicationRepository _repository;

    public RegisterApplicationHandler(
        IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<RegisterApplicationResult> HandleAsync(
        RegisterApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var application = ApplicationAggregate.Register(
            command.ProductId,
            command.AuthorityId,
            command.CountryId,
            command.ApplicantOrganizationId,
            command.DisplayName);

        await _repository.AddAsync(
            application,
            cancellationToken);

        return new RegisterApplicationResult(
            application.Id);
    }
}