using RegOS.Product.Contracts.Readers;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Application.Commands.CreateRegulatoryApplication;

public sealed class CreateRegulatoryApplicationHandler
{
    private readonly IProductReader _productReader;
    private readonly IRegulatoryApplicationRepository _repository;

    public CreateRegulatoryApplicationHandler(
        IProductReader productReader,
        IRegulatoryApplicationRepository repository)
    {
        _productReader = productReader;
        _repository = repository;
    }

    public async Task<CreateRegulatoryApplicationResult> HandleAsync(
        CreateRegulatoryApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _productReader.GetAsync(
            command.ProductId,
            cancellationToken);

        if (product is null)
        {
            throw new InvalidOperationException(
                $"Product '{command.ProductId}' does not exist.");
        }

        var regulatoryApplication = RegulatoryApplicationAggregate.Create(
            command.ProductId,
            command.AuthorityId,
            command.CountryId,
            command.ApplicantOrganizationId,
            command.Name);

        await _repository.AddAsync(
            regulatoryApplication,
            cancellationToken);

        return new CreateRegulatoryApplicationResult(
            regulatoryApplication.Id);
    }
}
