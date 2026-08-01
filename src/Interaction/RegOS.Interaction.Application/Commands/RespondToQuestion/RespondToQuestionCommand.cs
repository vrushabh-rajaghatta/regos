using RegOS.Interaction.Domain.Correspondence;

namespace RegOS.Interaction.Application.Commands.RespondToQuestion;

public sealed record RespondToQuestionCommand(
    HaCorrespondenceId CorrespondenceId,
    HaQuestionId QuestionId,
    string ResponseText,
    DateOnly OccurredOn,
    string? Note);
