using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.RemoveSubmissionRole;

public sealed record RemoveSubmissionRoleCommand(
    SubmissionId SubmissionId,
    SubmissionRoleId SubmissionRoleId);
