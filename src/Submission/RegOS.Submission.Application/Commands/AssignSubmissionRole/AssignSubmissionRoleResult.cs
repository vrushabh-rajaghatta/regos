using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.AssignSubmissionRole;

public sealed record AssignSubmissionRoleResult(
    SubmissionRoleId Id);
