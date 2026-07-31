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

    public async Task<MedicinalProduct?> GetByIdAsync(
        MedicinalProductId id,
        CancellationToken cancellationToken)
        => await _dbContext.MedicinalProducts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
