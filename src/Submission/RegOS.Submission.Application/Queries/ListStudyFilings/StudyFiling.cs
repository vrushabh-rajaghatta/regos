namespace RegOS.Submission.Application.Queries.ListStudyFilings;

/// <param name="Kind">
/// <c>Application</c> — the filing as a whole cites this study — or
/// <c>Sequence</c>, where a document placed in one sequence reports it.
/// </param>
/// <param name="SequenceNumber">
/// Formatted as eCTD writes it (<c>0000</c>), and null for a draft or for an
/// application row. The screen's word for a `Submission` is a sequence.
/// </param>
public sealed record StudyFiling(
    string Kind,
    Guid ApplicationId,
    string ApplicationName,
    string? ApplicationNumber,
    Guid? SubmissionId,
    string? SubmissionTitle,
    string? SequenceNumber);
