using RegOS.Product.Contracts.Models;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Contracts.Readers;

public interface IProductReader
{
    Task<bool> ExistsAsync(
        ProductId id,
        CancellationToken cancellationToken);

    Task<ProductInfo?> GetAsync(
        ProductId id,
        CancellationToken cancellationToken);
}
