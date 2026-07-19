namespace RegOS.Submission.Application.Queries.GetSubmissionSnapshot;

/// <summary>
/// One document in a published dossier, as exposed to read-side consumers. A frozen
/// reference to an immutable version, in its published position. Later milestones can
/// enrich it (filename, checksum, type, size) without touching the domain.
/// </summary>
public sealed record PublishedDocumentDto(
    int DisplayOrder,
    Guid DocumentVersionId);
