using RegOS.Interaction.Domain.Correspondence;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Application.Commands.RaiseQuestion;

public sealed class RaiseQuestionHandler
{
    private readonly IHaCorrespondenceRepository _repository;

    public RaiseQuestionHandler(IHaCorrespondenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<RaiseQuestionResult> HandleAsync(
        RaiseQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var correspondence =
            await _repository.GetByIdAsync(command.CorrespondenceId, cancellationToken)
            ?? throw new NotFoundException("Correspondence was not found.");

        var question = correspondence.RaiseQuestion(
            command.Number,
            command.Text,
            command.TargetResponseOn);

        await _repository.UpdateAsync(correspondence, cancellationToken);

        return new RaiseQuestionResult(question.Id);
    }
}
