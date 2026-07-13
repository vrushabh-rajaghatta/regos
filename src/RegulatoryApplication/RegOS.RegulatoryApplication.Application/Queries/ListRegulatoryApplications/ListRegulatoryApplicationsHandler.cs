using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Application.Queries.ListRegulatoryApplications;

public sealed class ListRegulatoryApplicationsHandler
{
    private readonly IRegulatoryApplicationRepository _repository;

    public ListRegulatoryApplicationsHandler(
        IRegulatoryApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ListRegulatoryApplicationsResult> HandleAsync(
        ProductId productId,
        CancellationToken cancellationToken)
    {
        var applications = await _repository.ListByProductAsync(
            productId,
            cancellationToken);

        var result = applications
            .Select(x => new RegulatoryApplicationInfo(
                x.Id.Value,
                x.Name,
                x.ApplicationNumber,
                x.Status.ToString()))
            .ToList();

        return new ListRegulatoryApplicationsResult(result);
    }
}
