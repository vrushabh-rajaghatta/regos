using RegOS.ProductDocument.Domain.IDs;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.AttachProductDocument;

/// <summary>
/// Attaches a Product Document to a Submission's dossier. The caller supplies
/// only the Product Document — the handler resolves its current version.
/// </summary>
public sealed record AttachProductDocumentCommand(
    SubmissionId SubmissionId,
    ProductDocumentId ProductDocumentId);
