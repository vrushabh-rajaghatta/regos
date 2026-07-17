using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;

namespace RegOS.ProductDocument.Domain.Repositories;

public interface IProductDocumentRepository
{
    Task AddAsync(
        ProductDocumentAggregate document,
        CancellationToken cancellationToken);

    Task<ProductDocumentAggregate?> GetByIdAsync(
        ProductDocumentId id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductDocumentAggregate>> GetByProductAsync(
        ProductId productId,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        ProductDocumentAggregate document,
        CancellationToken cancellationToken);
}
