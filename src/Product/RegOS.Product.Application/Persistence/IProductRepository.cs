using RegOS.Product.Domain.Product;


namespace RegOS.Product.Application.Persistence;

/// <summary>
/// Aggregates only. Reads for screens project from the database directly rather
/// than loading aggregates through here (ADR-006).
/// </summary>
public interface IProductRepository
{
    Task AddAsync(GlobalProduct product, CancellationToken cancellationToken);

    Task<GlobalProduct?> GetByIdAsync(
        GlobalProductId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(GlobalProduct product, CancellationToken cancellationToken);
}
