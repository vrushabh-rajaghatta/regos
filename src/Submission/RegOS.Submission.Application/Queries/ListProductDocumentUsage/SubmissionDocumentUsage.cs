namespace RegOS.Submission.Application.Queries.ListProductDocumentUsage;

/// <summary>
/// One submission's use of a Product Document — a cross-context read model
/// composed from SubmissionDocument, Submission, Application, Authority, and
/// the pinned DocumentVersion. Not a domain entity; the Product Document
/// aggregate is never loaded to build it.
///
/// Deliberately a flat record so it can grow later (submission status,
/// published-on, sequence number, validation status) without reshaping the
/// query's consumers.
/// </summary>
public sealed record SubmissionDocumentUsage(
    Guid SubmissionId,
    Guid ApplicationId,
    string SubmissionName,
    string SubmissionType,
    string Authority,
    int VersionNumber,
    DateTime AttachedOnUtc);
