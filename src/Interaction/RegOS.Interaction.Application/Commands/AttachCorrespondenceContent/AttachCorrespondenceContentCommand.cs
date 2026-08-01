using RegOS.Interaction.Domain.Correspondence;

namespace RegOS.Interaction.Application.Commands.AttachCorrespondenceContent;

public sealed record AttachCorrespondenceContentCommand(
    HaCorrespondenceId CorrespondenceId,
    string OriginalFileName,
    string ContentType,
    Stream Content);
