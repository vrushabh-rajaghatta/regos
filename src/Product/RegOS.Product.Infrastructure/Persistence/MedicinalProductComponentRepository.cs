using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Infrastructure.Persistence;

/// <inheritdoc cref="IMedicinalProductComponentRepository"/>
public sealed class MedicinalProductComponentRepository
    : IMedicinalProductComponentRepository
{
    private readonly RegOSDbContext _dbContext;

    public MedicinalProductComponentRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        MedicinalProductComponent component, CancellationToken cancellationToken)
    {
        _dbContext.MedicinalProductComponents.Add(component);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MedicinalProductComponent?> GetByIdAsync(
        MedicinalProductComponentId id, CancellationToken cancellationToken)
    {
        return await _dbContext.MedicinalProductComponents
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <remarks>
    /// Tracked, and all of them. The tree built from this is what refuses a
    /// cycle and an over-deep move, so a filtered or paged version of this
    /// method would quietly weaken both rules.
    /// </remarks>
    public async Task<IReadOnlyList<MedicinalProductComponent>> ListForMarketAsync(
        MedicinalProductId medicinalProductId, CancellationToken cancellationToken)
    {
        return await _dbContext.MedicinalProductComponents
            .Where(x => x.MedicinalProductId == medicinalProductId)
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveAsync(
        MedicinalProductComponent component, CancellationToken cancellationToken)
    {
        _dbContext.MedicinalProductComponents.Remove(component);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        MedicinalProductComponent component, CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
