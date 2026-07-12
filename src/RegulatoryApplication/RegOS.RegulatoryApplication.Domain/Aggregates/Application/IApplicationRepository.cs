using RegOS.Product.Domain.Product;

namespace RegOS.RegulatoryApplication.Domain.Aggregates.Application;

public interface IApplicationRepository
{
    Task AddAsync(
        Application application,
        CancellationToken cancellationToken);

    Task<Application?> GetByIdAsync(
        ApplicationId id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Application>> ListByProductAsync(
        ProductId productId,
        CancellationToken cancellationToken);
}