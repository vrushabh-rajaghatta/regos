using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.Entities;
using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.ProductDocument.Application.Queries.GetProductDocument;

public sealed class GetProductDocumentHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetProductDocumentHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductDocumentDetailDto?> HandleAsync(
        GlobalProductId globalProductId,
        ProductDocumentId documentId,
        CancellationToken cancellationToken)
    {
        // Document + its type and product names. Filtered by both ids so a
        // document can only be viewed under its owning product.
        var row = await (
            from document in _dbContext.ProductDocuments.AsNoTracking()
            where document.Id == documentId && document.GlobalProductId == globalProductId
            join documentType in _dbContext.DocumentTypes
                on document.DocumentTypeId equals documentType.Id
            join product in _dbContext.Products
                on document.GlobalProductId equals product.Id
            select new
            {
                document.Id,
                document.Name,
                DocumentTypeName = documentType.Name,
                document.Status,
                document.GlobalProductId,
                ProductName = product.Name,
                document.CreatedOnUtc,
                document.CurrentVersionId,
            }).SingleOrDefaultAsync(cancellationToken);

        if (row is null)
            return null;

        var currentVersion = await _dbContext.Set<DocumentVersion>()
            .AsNoTracking()
            .Where(v => v.Id == row.CurrentVersionId)
            .Select(v => new DocumentVersionDetailDto(
                v.VersionNumber,
                v.OriginalFileName,
                v.ContentType,
                v.FileSize,
                v.UploadedOnUtc))
            .FirstOrDefaultAsync(cancellationToken);

        return new ProductDocumentDetailDto(
            row.Id.Value,
            row.Name,
            row.DocumentTypeName,
            row.Status.ToString(),
            row.GlobalProductId.Value,
            row.ProductName.Value,
            row.CreatedOnUtc,
            currentVersion);
    }
}
