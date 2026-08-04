using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Infrastructure.Persistence;

/// <inheritdoc cref="IPackagedProductRepository"/>
public sealed class PackagedProductRepository : IPackagedProductRepository
{
    private readonly RegOSDbContext _dbContext;

    public PackagedProductRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        PackagedProduct pack, CancellationToken cancellationToken)
    {
        _dbContext.PackagedProducts.Add(pack);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <remarks>
    /// The history travels with the pack, because
    /// <see cref="PackagedProduct.ChangeMarketingStatus"/> compares the new date
    /// against the latest entry it holds. Without the <c>Include</c> the
    /// collection is empty, <c>Max</c> throws on an empty sequence, and the rule
    /// that business time moves forward would never run — the EPIC-004 S005
    /// failure mode exactly.
    /// </remarks>
    public async Task<PackagedProduct?> GetByIdAsync(
        PackagedProductId id, CancellationToken cancellationToken)
    {
        return await _dbContext.PackagedProducts
            .Include(x => x.MarketingStatusHistory)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(
        PackagedProduct pack, CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
