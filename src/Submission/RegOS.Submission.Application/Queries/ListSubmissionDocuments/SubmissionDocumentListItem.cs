namespace RegOS.Submission.Application.Queries.ListSubmissionDocuments;

/// <summary>
/// A row in the Submission Workspace's Documents tab. A read model joining the
/// attachment (SubmissionDocument) with the referenced Product Document, its
/// type, and the pinned version — the UI never loads aggregates to render it.
/// </summary>
public sealed record SubmissionDocumentListItem(
    Guid SubmissionDocumentId,
    int DisplayOrder,
    string DocumentName,
    string DocumentType,
    int VersionNumber,
    DateTime AttachedOnUtc);
