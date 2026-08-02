using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.ProductDocument.Application.Commands.UploadDocumentVersion;

public sealed record UploadDocumentVersionResult(
    DocumentVersionId Id,
    int VersionNumber);
