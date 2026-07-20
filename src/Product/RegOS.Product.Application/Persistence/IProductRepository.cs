using RegOS.Product.Domain.Product;

using ProductAggregate = RegOS.Product.Domain.Product.Product;

namespace RegOS.Product.Application.Persistence;

/// <summary>
/// Aggregates only. Reads for screens project from the database directly rather
/// than loading aggregates through here (ADR-006).
/// </summary>
public interface IProductRepository
{
    Task AddAsync(ProductAggregate product, CancellationToken cancellationToken);

    Task<ProductAggregate?> GetByIdAsync(
        ProductId id,
        CancellationToken cancellationToken);
}
