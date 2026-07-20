using RegOS.ProductDocument.Domain.Errors;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ProductDocument.Domain.Entities;

public sealed class DocumentVersion
{
    // Internal so that only the ProductDocument aggregate (same assembly)
    // can create a version. There is no path for application code to
    // instantiate a DocumentVersion independently of its aggregate root.
    internal DocumentVersion(
        DocumentVersionId id,
        int versionNumber,
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        string storagePath,
        string checksum,
        DateTime uploadedOnUtc)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new DomainException(ProductDocumentErrors.OriginalFileNameRequired);

        if (string.IsNullOrWhiteSpace(storedFileName))
            throw new DomainException(ProductDocumentErrors.StoredFileNameRequired);

        if (string.IsNullOrWhiteSpace(contentType))
            throw new DomainException(ProductDocumentErrors.ContentTypeRequired);

        if (string.IsNullOrWhiteSpace(storagePath))
            throw new DomainException(ProductDocumentErrors.InvalidStoragePath);

        if (fileSize <= 0)
            throw new DomainException(ProductDocumentErrors.InvalidFileSize);

        Id = id;
        VersionNumber = versionNumber;
        OriginalFileName = originalFileName.Trim();
        StoredFileName = storedFileName.Trim();
        ContentType = contentType.Trim();
        FileSize = fileSize;
        StoragePath = storagePath.Trim();
        Checksum = checksum;
        UploadedOnUtc = uploadedOnUtc;
    }

    public DocumentVersionId Id { get; }

    public int VersionNumber { get; }

    public string OriginalFileName { get; }

    public string StoredFileName { get; }

    public string ContentType { get; }

    public long FileSize { get; }

    public string StoragePath { get; }

    public string Checksum { get; }

    public DateTime UploadedOnUtc { get; }
}
