using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ProductDocument.Domain.Repositories;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;

namespace RegOS.ProductDocument.Infrastructure.Repositories;

public sealed class ProductDocumentRepository : IProductDocumentRepository
{
    private readonly RegOSDbContext _dbContext;

    public ProductDocumentRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        ProductDocumentAggregate document,
        CancellationToken cancellationToken)
    {
        _dbContext.ProductDocuments.Add(document);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductDocumentAggregate?> GetByIdAsync(
        ProductDocumentId id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ProductDocuments
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductDocumentAggregate>> GetByProductAsync(
        GlobalProductId globalProductId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ProductDocuments
            .Include(x => x.Versions)
            .Where(x => x.GlobalProductId == globalProductId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        ProductDocumentAggregate document,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
