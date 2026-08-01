using RegOS.Interaction.Domain.Correspondence;

namespace RegOS.Interaction.Application.Queries.GetCorrespondenceContent;

public sealed record GetCorrespondenceContentQuery(
    HaCorrespondenceId CorrespondenceId,
    CorrespondenceAttachmentId AttachmentId);
