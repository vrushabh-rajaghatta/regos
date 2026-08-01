using RegOS.Interaction.Domain.Correspondence;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Application.Commands.ResolveQuestion;

public sealed class ResolveQuestionHandler
{
    private readonly IHaCorrespondenceRepository _repository;

    public ResolveQuestionHandler(IHaCorrespondenceRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        ResolveQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var correspondence =
            await _repository.GetByIdAsync(command.CorrespondenceId, cancellationToken)
            ?? throw new NotFoundException("Correspondence was not found.");

        correspondence.ResolveQuestion(
            command.QuestionId,
            command.OccurredOn,
            command.Note);

        await _repository.UpdateAsync(correspondence, cancellationToken);
    }
}
