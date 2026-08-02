using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Queries.GetSubmissionChanges;

/// <summary>
/// What one filing did to the sequence before it.
/// </summary>
public sealed record GetSubmissionChangesQuery(SubmissionId SubmissionId);
