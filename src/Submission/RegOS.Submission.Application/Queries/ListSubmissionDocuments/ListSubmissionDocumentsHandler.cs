using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ProductDocument.Domain.Entities;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Queries.ListSubmissionDocuments;

public sealed class ListSubmissionDocumentsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListSubmissionDocumentsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns the submission's attachments in display order, or null when the
    /// submission does not exist (so the endpoint can 404 rather than return an
    /// empty list for a missing submission).
    /// </summary>
    public async Task<IReadOnlyList<SubmissionDocumentListItem>?> HandleAsync(
        SubmissionId submissionId,
        CancellationToken cancellationToken)
    {
        var submissionExists = await _dbContext.Submissions
            .AsNoTracking()
            .AnyAsync(s => s.Id == submissionId, cancellationToken);

        if (!submissionExists)
            return null;

        // The attachment carries only ids; names/type/version are read through
        // the referenced Product Document, its type, and the pinned version.
        // "SubmissionId" is a shadow FK on the child entity. The strongly-typed
        // id is materialized then unwrapped in memory (its converter has no SQL
        // translation for .Value).
        var rows = await (
            from attachment in _dbContext.Set<SubmissionDocument>().AsNoTracking()
            where EF.Property<SubmissionId>(attachment, "SubmissionId") == submissionId
            join document in _dbContext.ProductDocuments
                on attachment.ProductDocumentId equals document.Id
            join documentType in _dbContext.DocumentTypes
                on document.DocumentTypeId equals documentType.Id
            join version in _dbContext.Set<DocumentVersion>()
                on attachment.DocumentVersionId equals version.Id
            orderby attachment.DisplayOrder
            select new
            {
                attachment.Id,
                attachment.DisplayOrder,
                DocumentName = document.Name,
                DocumentType = documentType.Name,
                version.VersionNumber,
                attachment.AttachedAt,
            }).ToListAsync(cancellationToken);

        return rows
            .Select(row => new SubmissionDocumentListItem(
                row.Id.Value,
                row.DisplayOrder,
                row.DocumentName,
                row.DocumentType,
                row.VersionNumber,
                row.AttachedAt))
            .ToList();
    }
}
