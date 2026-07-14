using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.CreateSubmission;

public sealed record CreateSubmissionResult(
    SubmissionId Id);
