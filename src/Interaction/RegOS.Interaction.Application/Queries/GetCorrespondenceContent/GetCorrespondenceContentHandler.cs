using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;
using RegOS.Storage;

namespace RegOS.Interaction.Application.Queries.GetCorrespondenceContent;

public sealed class GetCorrespondenceContentHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly IFileStorage _fileStorage;

    public GetCorrespondenceContentHandler(
        RegOSDbContext dbContext,
        IFileStorage fileStorage)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
    }

    public async Task<CorrespondenceContent> HandleAsync(
        GetCorrespondenceContentQuery query,
        CancellationToken cancellationToken)
    {
        // Read through the letter, never the attachment table alone: the
        // tenant filter lives on the correspondence, and an attachment reached
        // directly would carry none (ADR-031).
        var attachment = await _dbContext.HaCorrespondence
            .AsNoTracking()
            .Where(x => x.Id == query.CorrespondenceId)
            .SelectMany(x => x.Attachments)
            .Where(x => x.Id == query.AttachmentId)
            .Select(x => new
            {
                x.StoragePath,
                x.ContentType,
                x.OriginalFileName
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("That attachment was not found.");

        var content = await _fileStorage.OpenReadAsync(
            attachment.StoragePath, cancellationToken);

        return new CorrespondenceContent(
            content,
            attachment.ContentType,
            attachment.OriginalFileName);
    }
}
