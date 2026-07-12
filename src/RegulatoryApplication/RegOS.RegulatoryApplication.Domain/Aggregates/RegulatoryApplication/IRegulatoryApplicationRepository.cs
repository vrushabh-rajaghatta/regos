using RegOS.Product.Domain.Product;

namespace RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

public interface IRegulatoryApplicationRepository
{
    Task AddAsync(
        RegulatoryApplication application,
        CancellationToken cancellationToken);

    Task<RegulatoryApplication?> GetByIdAsync(
        RegulatoryApplicationId id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RegulatoryApplication>> ListByProductAsync(
        ProductId productId,
        CancellationToken cancellationToken);
}
