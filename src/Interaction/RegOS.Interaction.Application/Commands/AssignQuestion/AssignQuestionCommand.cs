using RegOS.Interaction.Domain.Correspondence;
using RegOS.Platform.Contracts;

namespace RegOS.Interaction.Application.Commands.AssignQuestion;

public sealed record AssignQuestionCommand(
    HaCorrespondenceId CorrespondenceId,
    HaQuestionId QuestionId,
    UserId? OwnerUserId);
