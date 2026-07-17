using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.ProductDocument.Application.Commands.ActivateProductDocument;

/// <summary>
/// Activates a Product Document (Draft -> Active). The product is carried so
/// the handler can confirm the document belongs to the addressed product,
/// keeping the nested route honest.
/// </summary>
public sealed record ActivateProductDocumentCommand(
    ProductId ProductId,
    ProductDocumentId DocumentId);
