using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Infrastructure.Persistence;

/// <inheritdoc cref="IPackageItemRepository"/>
public sealed class PackageItemRepository : IPackageItemRepository
{
    private readonly RegOSDbContext _dbContext;

    public PackageItemRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PackageItem item, CancellationToken cancellationToken)
    {
        _dbContext.PackageItems.Add(item);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PackageItem?> GetByIdAsync(
        PackageItemId id, CancellationToken cancellationToken)
    {
        return await _dbContext.PackageItems
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <remarks>
    /// Tracked, and all of them. The tree built from this is what refuses a
    /// cycle and an over-deep move, so a filtered or paged version of this
    /// method would quietly weaken both rules.
    /// </remarks>
    public async Task<IReadOnlyList<PackageItem>> ListForPackAsync(
        PackagedProductId packagedProductId, CancellationToken cancellationToken)
    {
        return await _dbContext.PackageItems
            .Where(x => x.PackagedProductId == packagedProductId)
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveAsync(
        PackageItem item, CancellationToken cancellationToken)
    {
        _dbContext.PackageItems.Remove(item);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        PackageItem item, CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
