using RegOS.Interaction.Domain.Correspondence;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Application.Commands.AssignQuestion;

public sealed class AssignQuestionHandler
{
    private readonly IHaCorrespondenceRepository _repository;

    public AssignQuestionHandler(IHaCorrespondenceRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        AssignQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var correspondence =
            await _repository.GetByIdAsync(command.CorrespondenceId, cancellationToken)
            ?? throw new NotFoundException("Correspondence was not found.");

        // Null clears the assignment — unassigning is a real act, not an error.
        correspondence.AssignQuestion(command.QuestionId, command.OwnerUserId);

        await _repository.UpdateAsync(correspondence, cancellationToken);
    }
}
