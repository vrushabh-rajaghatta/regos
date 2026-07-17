namespace RegOS.ProductDocument.Domain.Errors;

public static class ProductDocumentErrors
{
    // ProductDocument
    public const string ProductRequired =
        "Product is required.";

    public const string DocumentTypeRequired =
        "Document Type is required.";

    public const string DocumentNameRequired =
        "Document name is required.";

    public const string DocumentNameTooLong =
        "Document name must be 200 characters or fewer.";

    public const string DocumentAlreadyHasInitialVersion =
        "Document already has an initial version.";

    public const string DocumentHasNoInitialVersion =
        "Document has no initial version.";

    public const string CannotActivateWithoutVersion =
        "A document cannot be activated before a version is uploaded.";

    public const string CannotArchiveDraft =
        "A draft document cannot be archived.";

    public const string DocumentAlreadyActive =
        "Document is already active.";

    public const string DocumentArchived =
        "Document is archived.";

    // DocumentVersion
    public const string OriginalFileNameRequired =
        "Original file name is required.";

    public const string StoredFileNameRequired =
        "Stored file name is required.";

    public const string ContentTypeRequired =
        "Content type is required.";

    public const string InvalidStoragePath =
        "Storage path is required.";

    public const string InvalidFileSize =
        "File size must be greater than zero.";
}
