namespace RegOS.Submission.Application.Queries.GetSubmissionSnapshot;

/// <summary>
/// The published dossier as seen by read-side consumers — "exactly what was
/// published." Named for the business capability (a published submission), not the
/// storage mechanism (a snapshot), so the contract stays stable even if the response
/// is later served from an eCTD archive, a cache, or elsewhere.
/// </summary>
/// <remarks>
/// PublishedBy is intentionally absent until the project has a current-user identity
/// to record. It will be added here (a read-side change only) once it exists.
/// </remarks>
public sealed record PublishedSubmissionDto(
    Guid SubmissionId,
    DateTimeOffset? PublishedAt,
    IReadOnlyList<PublishedDocumentDto> Documents);
