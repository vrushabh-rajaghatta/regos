using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ProductDocument.Application.Exceptions;
using RegOS.ProductDocument.Application.Storage;
using RegOS.ProductDocument.Domain.Repositories;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;

namespace RegOS.ProductDocument.Application.Commands.UploadProductDocument;

public sealed class UploadProductDocumentHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly IProductDocumentRepository _repository;
    private readonly IFileStorage _fileStorage;

    public UploadProductDocumentHandler(
        RegOSDbContext dbContext,
        IProductDocumentRepository repository,
        IFileStorage fileStorage)
    {
        _dbContext = dbContext;
        _repository = repository;
        _fileStorage = fileStorage;
    }

    public async Task<UploadProductDocumentResult> HandleAsync(
        UploadProductDocumentCommand command,
        CancellationToken cancellationToken)
    {
        // Buffer once so we can both hash and store the exact same bytes.
        using var buffer = new MemoryStream();
        await command.Content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        if (bytes.LongLength == 0)
            throw new BusinessRuleViolationException(
                ProductDocumentUploadErrors.EmptyFile);

        // Rule 1 — Product exists (the addressed resource).
        var productExists = await _dbContext.Products
            .AnyAsync(p => p.Id == command.ProductId, cancellationToken);

        if (!productExists)
            throw new ProductNotFoundException(
                ProductDocumentUploadErrors.ProductDoesNotExist);

        // Rule 2 — Document Type exists and is active.
        var documentType = await _dbContext.DocumentTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(
                t => t.Id == command.DocumentTypeId,
                cancellationToken);

        if (documentType is null)
            throw new BusinessRuleViolationException(
                ProductDocumentUploadErrors.DocumentTypeDoesNotExist);

        if (!documentType.IsActive)
            throw new BusinessRuleViolationException(
                ProductDocumentUploadErrors.DocumentTypeInactive);

        // Create the aggregate first so we have its id for the storage path.
        var document = ProductDocumentAggregate.Create(
            command.ProductId,
            command.DocumentTypeId,
            command.Name);

        // Deterministic stored filename + relative path mirroring ownership.
        // Original filename is preserved separately on the version.
        var extension = Path.GetExtension(command.OriginalFileName);
        var storedFileName = $"v1{extension}";
        var relativePath =
            $"products/{command.ProductId.Value}/{document.Id.Value}/{storedFileName}";

        var checksum = Convert.ToHexString(SHA256.HashData(bytes))
            .ToLowerInvariant();

        await using (var content = new MemoryStream(bytes))
        {
            await _fileStorage.SaveAsync(relativePath, content, cancellationToken);
        }

        document.AddInitialVersion(
            originalFileName: command.OriginalFileName,
            storedFileName: storedFileName,
            contentType: command.ContentType,
            fileSize: bytes.LongLength,
            storagePath: relativePath,
            checksum: checksum);

        try
        {
            await _repository.AddAsync(document, cancellationToken);
        }
        catch
        {
            // Persistence failed after the file was written — remove the
            // orphaned file so storage does not drift from the database.
            await _fileStorage.DeleteAsync(relativePath, cancellationToken);
            throw;
        }

        return new UploadProductDocumentResult(document.Id);
    }
}
