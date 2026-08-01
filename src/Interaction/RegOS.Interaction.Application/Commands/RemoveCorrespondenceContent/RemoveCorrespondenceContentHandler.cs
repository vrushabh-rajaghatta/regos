using RegOS.Interaction.Domain.Correspondence;
using RegOS.SharedKernel.Exceptions;
using RegOS.Storage;

namespace RegOS.Interaction.Application.Commands.RemoveCorrespondenceContent;

public sealed class RemoveCorrespondenceContentHandler
{
    private readonly IHaCorrespondenceRepository _repository;
    private readonly IFileStorage _fileStorage;

    public RemoveCorrespondenceContentHandler(
        IHaCorrespondenceRepository repository,
        IFileStorage fileStorage)
    {
        _repository = repository;
        _fileStorage = fileStorage;
    }

    public async Task HandleAsync(
        RemoveCorrespondenceContentCommand command,
        CancellationToken cancellationToken)
    {
        var correspondence =
            await _repository.GetByIdAsync(command.CorrespondenceId, cancellationToken)
            ?? throw new NotFoundException("Correspondence was not found.");

        // The aggregate decides whether the attachment is really its own, and
        // throws NotFound if it is not — an attachment id from another letter
        // must not delete a file.
        var removed = correspondence.RemoveContent(command.AttachmentId);

        await _repository.UpdateAsync(correspondence, cancellationToken);

        // The record is the truth; the file follows it. If this throws, the
        // correspondence is already correct and an orphaned blob remains.
        await _fileStorage.DeleteAsync(removed.StoragePath, cancellationToken);
    }
}
