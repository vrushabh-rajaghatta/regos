using System.Security.Cryptography;

using RegOS.ProductDocument.Domain.Repositories;
using RegOS.SharedKernel.Exceptions;
using RegOS.Storage;

namespace RegOS.ProductDocument.Application.Commands.UploadDocumentVersion;

/// <summary>
/// Adds the next version of a document that already exists.
/// </summary>
/// <remarks>
/// <b>Added at EPIC-004 S002, and it is older than that.</b> The aggregate has
/// carried <c>AddNewVersion</c> since EPIC-003 with a comment saying it was
/// modelled but not exposed. Nothing reached it, so a revised document could not
/// be recorded at all — and the cumulative filing model (ADR-045) turns that
/// from a missing convenience into a missing gesture: *this document has a new
/// version, file it again* is the single most common thing a sequence does.
/// <para>
/// The aggregate owns the version number; this only supplies the bytes.
/// </para>
/// </remarks>
public sealed class UploadDocumentVersionHandler
{
    private readonly IProductDocumentRepository _repository;
    private readonly IFileStorage _fileStorage;

    public UploadDocumentVersionHandler(
        IProductDocumentRepository repository,
        IFileStorage fileStorage)
    {
        _repository = repository;
        _fileStorage = fileStorage;
    }

    public async Task<UploadDocumentVersionResult> HandleAsync(
        UploadDocumentVersionCommand command,
        CancellationToken cancellationToken)
    {
        // Buffer once so the bytes hashed and the bytes stored are identical.
        using var buffer = new MemoryStream();
        await command.Content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        if (bytes.LongLength == 0)
            throw new DomainException(ProductDocumentUploadErrors.EmptyFile);

        var document = await _repository.GetByIdAsync(
            command.ProductDocumentId, cancellationToken);

        if (document is null)
            throw new NotFoundException("The document was not found.");

        // The next number is the aggregate's to know, but the storage path
        // needs it before AddNewVersion runs. Read from the versions the
        // aggregate already holds rather than duplicating the rule: if they
        // ever disagree, the aggregate's guard is what fails, not the file.
        var nextVersionNumber = document.Versions.Max(v => v.VersionNumber) + 1;

        var extension = Path.GetExtension(command.OriginalFileName);
        var storedFileName = $"v{nextVersionNumber}{extension}";
        var relativePath =
            $"products/{document.GlobalProductId.Value}/{document.Id.Value}/{storedFileName}";

        var checksum = Convert.ToHexString(SHA256.HashData(bytes))
            .ToLowerInvariant();

        await using (var content = new MemoryStream(bytes))
        {
            await _fileStorage.SaveAsync(relativePath, content, cancellationToken);
        }

        document.AddNewVersion(
            originalFileName: command.OriginalFileName,
            storedFileName: storedFileName,
            contentType: command.ContentType,
            fileSize: bytes.LongLength,
            storagePath: relativePath,
            checksum: checksum);

        try
        {
            await _repository.UpdateAsync(document, cancellationToken);
        }
        catch
        {
            // Persistence failed after the file was written — remove the
            // orphaned file so storage does not drift from the database.
            await _fileStorage.DeleteAsync(relativePath, cancellationToken);
            throw;
        }

        return new UploadDocumentVersionResult(
            document.CurrentVersionId!.Value, nextVersionNumber);
    }
}
