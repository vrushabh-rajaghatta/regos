namespace RegOS.Api.Endpoints.Submissions;

public sealed record CreateSubmissionRequest(
    Guid SubmissionTypeId,
    string Name);
