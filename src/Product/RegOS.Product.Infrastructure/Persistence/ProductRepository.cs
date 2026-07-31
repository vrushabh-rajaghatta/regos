using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Application.Persistence;
using RegOS.Product.Domain.Product;


namespace RegOS.Product.Infrastructure.Persistence;

/// <summary>
/// Saves within each method, matching the convention the Platform repositories
/// established. The IUnitOfWork abstraction this context used to carry was
/// removed: one DbContext per request already is the unit of work, and no
/// handler ever needed to compose several repositories into one commit.
/// </summary>
public sealed class ProductRepository : IProductRepository
{
    private readonly RegOSDbContext _dbContext;

    public ProductRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        GlobalProduct product,
        CancellationToken cancellationToken)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        GlobalProduct product,
        CancellationToken cancellationToken)
    {
        _dbContext.Products.Update(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<GlobalProduct?> GetByIdAsync(
        GlobalProductId id,
        CancellationToken cancellationToken)
        => await _dbContext.Products
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
