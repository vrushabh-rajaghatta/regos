using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.ProductDocument.Application.Commands.ArchiveProductDocument;

/// <summary>
/// Archives a Product Document (Active -> Archived). The product is carried so
/// the handler can confirm the document belongs to the addressed product,
/// keeping the nested route honest.
/// </summary>
public sealed record ArchiveProductDocumentCommand(
    ProductId ProductId,
    ProductDocumentId DocumentId);
