using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.PublishSubmission;

public sealed record PublishSubmissionCommand(SubmissionId SubmissionId);
