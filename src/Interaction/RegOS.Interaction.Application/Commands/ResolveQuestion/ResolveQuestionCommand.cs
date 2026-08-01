using RegOS.Interaction.Domain.Correspondence;

namespace RegOS.Interaction.Application.Commands.ResolveQuestion;

public sealed record ResolveQuestionCommand(
    HaCorrespondenceId CorrespondenceId,
    HaQuestionId QuestionId,
    DateOnly OccurredOn,
    string? Note);
