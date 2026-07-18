namespace RegOS.Submission.Application.Queries.ListSubmissions;

public sealed record SubmissionSummary(
    Guid Id,
    string Title,
    string Status,
    string SubmissionTypeName,
    DateTime CreatedOn);
