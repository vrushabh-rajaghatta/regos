using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.ChangeSubmissionFormat;

public sealed class ChangeSubmissionFormatHandler
{
    private readonly ISubmissionRepository _repository;

    public ChangeSubmissionFormatHandler(ISubmissionRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        ChangeSubmissionFormatCommand command,
        CancellationToken cancellationToken)
    {
        var submission = await _repository.GetByIdAsync(
            command.SubmissionId,
            cancellationToken);

        if (submission is null)
            throw new NotFoundException(
                SubmissionRuleErrors.SubmissionDoesNotExist);

        // Every rule here is visible from the aggregate's own state — whether
        // it is still a draft, and whether the value is one RegOS knows. No
        // reference-data lookup, so no DbContext.
        submission.ChangeFormat(command.Format);

        await _repository.UpdateAsync(submission, cancellationToken);
    }
}
