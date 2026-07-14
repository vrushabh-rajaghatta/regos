namespace RegOS.Submission.Application.Queries.ListSubmissions;

public sealed record SubmissionSummary(
    Guid Id,
    string Name,
    string Status,
    string SubmissionTypeName,
    DateTime CreatedOn);
