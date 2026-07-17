namespace RegOS.ProductDocument.Application.Queries.ListProductDocuments;

public sealed record ProductDocumentSummary(
    Guid Id,
    string Name,
    string DocumentTypeName,
    string Status,
    int? CurrentVersionNumber,
    DateTime CreatedOnUtc);
