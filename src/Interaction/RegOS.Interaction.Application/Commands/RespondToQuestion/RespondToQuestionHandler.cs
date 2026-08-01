using RegOS.Interaction.Domain.Correspondence;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Application.Commands.RespondToQuestion;

public sealed class RespondToQuestionHandler
{
    private readonly IHaCorrespondenceRepository _repository;

    public RespondToQuestionHandler(IHaCorrespondenceRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        RespondToQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var correspondence =
            await _repository.GetByIdAsync(command.CorrespondenceId, cancellationToken)
            ?? throw new NotFoundException("Correspondence was not found.");

        correspondence.RespondToQuestion(
            command.QuestionId,
            command.ResponseText,
            command.OccurredOn,
            command.Note);

        await _repository.UpdateAsync(correspondence, cancellationToken);
    }
}
