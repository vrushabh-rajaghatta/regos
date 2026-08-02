using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Queries.ListSubmissionRoles;

/// <summary>
/// Who is named on this submission (ADR-048).
/// </summary>
public sealed record ListSubmissionRolesQuery(
    SubmissionId SubmissionId);
