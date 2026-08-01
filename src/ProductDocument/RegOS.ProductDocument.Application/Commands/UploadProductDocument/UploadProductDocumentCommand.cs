using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.DocumentType;

namespace RegOS.ProductDocument.Application.Commands.UploadProductDocument;

public sealed record UploadProductDocumentCommand(
    GlobalProductId GlobalProductId,
    DocumentTypeId DocumentTypeId,
    string Name,
    string OriginalFileName,
    string ContentType,
    Stream Content);
