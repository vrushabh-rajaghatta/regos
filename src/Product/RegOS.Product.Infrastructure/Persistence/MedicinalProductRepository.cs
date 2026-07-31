using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Infrastructure.Persistence;

public sealed class MedicinalProductRepository : IMedicinalProductRepository
{
    private readonly RegOSDbContext _dbContext;

    public MedicinalProductRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        MedicinalProduct medicinalProduct,
        CancellationToken cancellationToken)
    {
        await _dbContext.MedicinalProducts.AddAsync(
            medicinalProduct, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Tracked, with trade names — see <see cref="IMedicinalProductRepository"/>
    /// for why they are always loaded rather than on request.
    /// </summary>
    public async Task<MedicinalProduct?> GetByIdAsync(
        MedicinalProductId id,
        CancellationToken cancellationToken)
        => await _dbContext.MedicinalProducts
            .Include(x => x.TradeNames)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(
        MedicinalProduct medicinalProduct,
        CancellationToken cancellationToken)
        => await _dbContext.SaveChangesAsync(cancellationToken);
}
