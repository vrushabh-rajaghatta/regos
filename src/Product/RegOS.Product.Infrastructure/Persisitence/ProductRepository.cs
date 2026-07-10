namespace RegOS.Product.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using RegOS.Product.Application.Persistence;
using RegOS.Product.Domain.Product;

public sealed class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _dbContext;

    public ProductRepository(ProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products.OrderBy(p => p.Name).ToListAsync(cancellationToken);
    }
}