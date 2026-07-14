namespace RegOS.Submission.Application.Queries.GetSubmission;

public sealed record SubmissionDetailDto(
    Guid Id,
    string Name,
    Guid ApplicationId,
    string ApplicationName,
    Guid SubmissionTypeId,
    string SubmissionTypeName,
    string Status,
    DateTime CreatedOn);
