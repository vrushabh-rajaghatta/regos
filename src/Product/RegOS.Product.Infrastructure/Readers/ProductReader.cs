using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Contracts.Models;
using RegOS.Product.Contracts.Readers;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Infrastructure.Readers;

public sealed class ProductReader : IProductReader
{
    private readonly RegOSDbContext _dbContext;

    public ProductReader(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(
        ProductId id,
        CancellationToken cancellationToken)
    {
        return _dbContext.Products
            .AnyAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<ProductInfo?> GetAsync(
        ProductId id,
        CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (product is null)
        {
            return null;
        }

        return new ProductInfo(
            product.Id,
            product.Name.Value,
            product.Type,
            product.Status);
    }
}
