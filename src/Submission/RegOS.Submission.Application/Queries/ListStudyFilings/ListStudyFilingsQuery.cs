namespace RegOS.Submission.Application.Queries.ListStudyFilings;

/// <summary>
/// "Which filings cite this study?" — the inverse of
/// <c>ListApplicationStudies</c>, and the half that makes a citation visible
/// from both ends.
/// </summary>
/// <param name="StudyId">
/// A plain guid: the caller has a study and wants its filings, and which of the
/// two aggregates it came from does not change the question.
/// </param>
public sealed record ListStudyFilingsQuery(Guid StudyId);
