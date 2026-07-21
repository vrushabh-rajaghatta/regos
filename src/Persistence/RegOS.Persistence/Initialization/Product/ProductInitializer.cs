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
        // IgnoreQueryFilters: startup has no tenant, so the filter would
        // report an empty table every boot and this would insert duplicates
        // until the unique (TenantId, Code) index refused (ADR-031).
        if (await _dbContext.Products
                .IgnoreQueryFilters()
                .AnyAsync(cancellationToken))
            return;

        _dbContext.Products.AddRange(Products.Data);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
