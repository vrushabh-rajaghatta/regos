using RegOS.Interaction.Domain.Correspondence;

namespace RegOS.Interaction.Application.Commands.RaiseQuestion;

public sealed record RaiseQuestionCommand(
    HaCorrespondenceId CorrespondenceId,
    string Number,
    string Text,
    DateOnly? TargetResponseOn);
