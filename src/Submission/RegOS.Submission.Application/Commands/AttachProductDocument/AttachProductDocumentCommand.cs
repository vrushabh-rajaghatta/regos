using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.AttachProductDocument;

/// <summary>
/// Attaches a Product Document to a Submission's dossier. The caller supplies
/// only the Product Document — the handler resolves its current version.
/// </summary>
/// <param name="TemplateSectionId">
/// Optionally, where in the dossier the document lands. "Put this into 3.2.S.2"
/// is one user action; requiring a second call to place it would manufacture an
/// unplaced state that exists only because of API shape.
/// </param>
public sealed record AttachProductDocumentCommand(
    SubmissionId SubmissionId,
    ProductDocumentId ProductDocumentId,
    TemplateSectionId? TemplateSectionId = null);
