using RegOS.ProductDocument.Domain.Errors;
using RegOS.ProductDocument.Domain.IDs;

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
            throw new ArgumentException(
                ProductDocumentErrors.OriginalFileNameRequired,
                nameof(originalFileName));

        if (string.IsNullOrWhiteSpace(storedFileName))
            throw new ArgumentException(
                ProductDocumentErrors.StoredFileNameRequired,
                nameof(storedFileName));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException(
                ProductDocumentErrors.ContentTypeRequired,
                nameof(contentType));

        if (string.IsNullOrWhiteSpace(storagePath))
            throw new ArgumentException(
                ProductDocumentErrors.InvalidStoragePath,
                nameof(storagePath));

        if (fileSize <= 0)
            throw new ArgumentException(
                ProductDocumentErrors.InvalidFileSize,
                nameof(fileSize));

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
