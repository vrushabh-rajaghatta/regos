using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Infrastructure.Persistence;

/// <inheritdoc cref="IPharmaceuticalProductDetailRepository"/>
public sealed class PharmaceuticalProductDetailRepository
    : IPharmaceuticalProductDetailRepository
{
    private readonly RegOSDbContext _dbContext;

    public PharmaceuticalProductDetailRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        PharmaceuticalProductDetail detail,
        CancellationToken cancellationToken)
    {
        _dbContext.PharmaceuticalProductDetails.Add(detail);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PharmaceuticalProductDetail?> GetByIdAsync(
        PharmaceuticalProductDetailId id,
        CancellationToken cancellationToken)
    {
        // Two Includes, each load-bearing for a different rule.
        //
        // Routes: Restate replaces the collection wholesale, so a load without
        // them would clear an empty list and silently drop every route the
        // presentation already had.
        //
        // Ingredients: the aggregate reasons across the whole composition —
        // it refuses a substance already present, and refuses to leave a
        // formulation with excipients and no active. Both read the list, so a
        // load without it would let a duplicate through and hollow out a
        // composition without noticing. This is the load EPIC-019 got wrong
        // once already.
        //
        // The dose form, unit and each ingredient's strength are owned
        // one-to-one and load with their row.
        return await _dbContext.PharmaceuticalProductDetails
            .Include(x => x.RoutesOfAdministration)
            .Include(x => x.Ingredients)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(
        PharmaceuticalProductDetail detail,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
