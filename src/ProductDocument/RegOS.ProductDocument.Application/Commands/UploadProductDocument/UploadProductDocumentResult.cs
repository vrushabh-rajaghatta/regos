using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.ProductDocument.Application.Commands.UploadProductDocument;

public sealed record UploadProductDocumentResult(
    ProductDocumentId Id);
