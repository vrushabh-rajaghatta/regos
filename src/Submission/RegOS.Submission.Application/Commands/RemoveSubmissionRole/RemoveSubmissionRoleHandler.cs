using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.RemoveSubmissionRole;

public sealed class RemoveSubmissionRoleHandler
{
    private readonly ISubmissionRepository _repository;

    public RemoveSubmissionRoleHandler(ISubmissionRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        RemoveSubmissionRoleCommand command,
        CancellationToken cancellationToken)
    {
        var submission = await _repository.GetByIdAsync(
            command.SubmissionId,
            cancellationToken);

        if (submission is null)
            throw new NotFoundException(
                SubmissionRuleErrors.SubmissionDoesNotExist);

        // Both rules — still a draft, and this naming belongs to this
        // submission — are visible from the aggregate's own state, so there is
        // no reference-data lookup and no DbContext.
        submission.RemoveRole(command.SubmissionRoleId);

        await _repository.UpdateAsync(submission, cancellationToken);
    }
}
