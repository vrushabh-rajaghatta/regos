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
        // Routes included because Restate replaces the collection wholesale: a
        // load without them would clear an empty list and silently drop every
        // route the presentation already had. The dose form and unit are owned
        // one-to-one and load with the row.
        return await _dbContext.PharmaceuticalProductDetails
            .Include(x => x.RoutesOfAdministration)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(
        PharmaceuticalProductDetail detail,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
