using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.PlaceSubmissionDocument;

/// <summary>
/// Moves an already-attached document to a section of the dossier — or, with a
/// null <paramref name="TemplateSectionId"/>, takes it out of the structure
/// while leaving it attached.
/// </summary>
/// <param name="TemplateSectionId">
/// Null clears the placement rather than meaning "unchanged": this expresses the
/// whole placement, so a caller can always state the end state it wants.
/// </param>
public sealed record PlaceSubmissionDocumentCommand(
    SubmissionId SubmissionId,
    SubmissionDocumentId SubmissionDocumentId,
    TemplateSectionId? TemplateSectionId);
