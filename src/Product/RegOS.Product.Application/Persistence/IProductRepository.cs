namespace RegOS.Product.Application.Persistence;

using RegOS.Product.Domain.Product;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(
    ProductId id,
    CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken = default);
}