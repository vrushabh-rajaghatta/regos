using RegOS.Submission.Domain.Submission;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Submission.Application.Commands.RemoveProductDocument;

/// <summary>
/// Removal needs no Product Document lookup — the aggregate already owns the
/// attachment and enforces the draft and "must be attached" rules.
/// </summary>
public sealed class RemoveProductDocumentHandler
{
    private readonly ISubmissionRepository _repository;

    public RemoveProductDocumentHandler(ISubmissionRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        RemoveProductDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var submission = await _repository.GetByIdAsync(
            command.SubmissionId,
            cancellationToken);

        if (submission is null)
            throw new NotFoundException(
                SubmissionRuleErrors.SubmissionDoesNotExist);

        submission.RemoveDocument(command.SubmissionDocumentId);

        await _repository.UpdateAsync(submission, cancellationToken);
    }
}
