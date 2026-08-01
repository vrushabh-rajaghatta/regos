using RegOS.Interaction.Domain.Correspondence;

namespace RegOS.Interaction.Application.Commands.RemoveCorrespondenceContent;

public sealed record RemoveCorrespondenceContentCommand(
    HaCorrespondenceId CorrespondenceId,
    CorrespondenceAttachmentId AttachmentId);
