namespace RegOS.Submission.Application.Queries.ListAttachableProductDocuments;

/// <summary>
/// A Product Document that may be attached to a submission — a regulatory
/// asset the user can choose from. The query already filters to Active
/// documents of the submission's product that are not yet attached, so every
/// item shown is a valid choice.
/// </summary>
public sealed record AttachableProductDocument(
    Guid ProductDocumentId,
    string Name,
    string DocumentType,
    int? CurrentVersionNumber,
    string Status,
    DateTime CreatedOnUtc);
