using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.RemoveProductDocument;

/// <summary>
/// Removes an attachment from a Submission's dossier. Targets the attachment
/// (SubmissionDocument), not the underlying Product Document.
/// </summary>
public sealed record RemoveProductDocumentCommand(
    SubmissionId SubmissionId,
    SubmissionDocumentId SubmissionDocumentId);
