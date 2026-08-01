using RegOS.Interaction.Domain.Correspondence;

namespace RegOS.Interaction.Application.Commands.AttachCorrespondenceContent;

public sealed record AttachCorrespondenceContentResult(
    CorrespondenceAttachmentId AttachmentId);
