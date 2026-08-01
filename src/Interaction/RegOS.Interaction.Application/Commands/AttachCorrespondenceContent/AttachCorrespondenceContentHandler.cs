using RegOS.Interaction.Domain.Correspondence;
using RegOS.SharedKernel.Exceptions;
using RegOS.Storage;

namespace RegOS.Interaction.Application.Commands.AttachCorrespondenceContent;

public sealed class AttachCorrespondenceContentHandler
{
    private readonly IHaCorrespondenceRepository _repository;
    private readonly IFileStorage _fileStorage;

    public AttachCorrespondenceContentHandler(
        IHaCorrespondenceRepository repository,
        IFileStorage fileStorage)
    {
        _repository = repository;
        _fileStorage = fileStorage;
    }

    public async Task<AttachCorrespondenceContentResult> HandleAsync(
        AttachCorrespondenceContentCommand command,
        CancellationToken cancellationToken)
    {
        var correspondence =
            await _repository.GetByIdAsync(command.CorrespondenceId, cancellationToken)
            ?? throw new NotFoundException("Correspondence was not found.");

        // Written first, recorded second: a row pointing at bytes that were
        // never saved is the worse of the two failures. The reverse leaves an
        // orphaned file, which is recoverable.
        var attachmentId = CorrespondenceAttachmentId.New();

        var relativePath =
            $"correspondence/{command.CorrespondenceId.Value}/{attachmentId.Value}";

        await _fileStorage.SaveAsync(relativePath, command.Content, cancellationToken);

        var attachment = correspondence.AttachContent(
            command.OriginalFileName,
            command.ContentType,
            command.Content.Length,
            relativePath);

        await _repository.UpdateAsync(correspondence, cancellationToken);

        return new AttachCorrespondenceContentResult(attachment.Id);
    }
}
