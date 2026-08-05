using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.Entities;

namespace RegOS.ProductDocument.Application.Queries.ListProductDocuments;

public sealed class ListProductDocumentsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListProductDocumentsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductDocumentSummary>> HandleAsync(
        GlobalProductId globalProductId,
        CancellationToken cancellationToken)
    {
        // Lightweight summaries for the workspace list — the document type
        // name and the current version's number, joined without loading the
        // whole version collection. Status is materialized then stringified.
        var rows = await (
            from document in _dbContext.ProductDocuments.AsNoTracking()
            where document.GlobalProductId == globalProductId
            join documentType in _dbContext.DocumentTypes
                on document.DocumentTypeId equals documentType.Id
            orderby document.CreatedOnUtc descending, document.Id
            select new
            {
                document.Id,
                document.Name,
                DocumentTypeName = documentType.Name,
                document.Status,
                CurrentVersionNumber = _dbContext.Set<DocumentVersion>()
                    .Where(v => v.Id == document.CurrentVersionId)
                    .Select(v => (int?)v.VersionNumber)
                    .FirstOrDefault(),
                document.CreatedOnUtc,
            }).ToListAsync(cancellationToken);

        return rows
            .Select(row => new ProductDocumentSummary(
                row.Id.Value,
                row.Name,
                row.DocumentTypeName,
                row.Status.ToString(),
                row.CurrentVersionNumber,
                row.CreatedOnUtc))
            .ToList();
    }
}
