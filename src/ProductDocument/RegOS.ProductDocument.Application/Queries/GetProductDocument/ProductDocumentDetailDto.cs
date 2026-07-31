namespace RegOS.ProductDocument.Application.Queries.GetProductDocument;

public sealed record ProductDocumentDetailDto(
    Guid Id,
    string Name,
    string DocumentTypeName,
    string Status,
    Guid GlobalProductId,
    string ProductName,
    DateTime CreatedOnUtc,
    DocumentVersionDetailDto? CurrentVersion);

public sealed record DocumentVersionDetailDto(
    int VersionNumber,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    DateTime UploadedOnUtc);
