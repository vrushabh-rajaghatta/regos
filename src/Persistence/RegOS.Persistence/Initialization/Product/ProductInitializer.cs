using Microsoft.EntityFrameworkCore;

namespace RegOS.Persistence.Initialization.Product;

public sealed class ProductInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public ProductInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Products.AnyAsync(cancellationToken))
            return;

        _dbContext.Products.AddRange(Products.Data);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
