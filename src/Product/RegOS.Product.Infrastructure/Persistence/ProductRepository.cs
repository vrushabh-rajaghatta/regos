using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Application.Persistence;
using RegOS.Product.Domain.Product;

using ProductAggregate = RegOS.Product.Domain.Product.Product;

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
        ProductAggregate product,
        CancellationToken cancellationToken)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductAggregate?> GetByIdAsync(
        ProductId id,
        CancellationToken cancellationToken)
        => await _dbContext.Products
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ProductAggregate>> ListAsync(
        CancellationToken cancellationToken)
        => await _dbContext.Products
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
}
