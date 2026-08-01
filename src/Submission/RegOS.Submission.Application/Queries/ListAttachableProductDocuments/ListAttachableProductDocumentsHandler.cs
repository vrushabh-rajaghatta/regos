using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ProductDocument.Domain.Entities;
using RegOS.ProductDocument.Domain.Enums;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Queries.ListAttachableProductDocuments;

public sealed class ListAttachableProductDocumentsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListAttachableProductDocumentsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Active Product Documents of the submission's product that are not yet
    /// attached. Returns null when the submission does not exist. Applying the
    /// filters here keeps the picker simple and prevents invalid choices.
    /// </summary>
    public async Task<IReadOnlyList<AttachableProductDocument>?> HandleAsync(
        SubmissionId submissionId,
        CancellationToken cancellationToken)
    {
        var submission = await _dbContext.Submissions
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

        if (submission is null)
            return null;

        var application = await _dbContext.RegulatoryApplications
            .AsNoTracking()
            .SingleOrDefaultAsync(
                a => a.Id == submission.ApplicationId,
                cancellationToken);

        if (application is null)
            return null;

        var globalProductId = application.GlobalProductId;

        // Documents already in this submission's dossier — excluded so the
        // picker never offers a duplicate.
        var attachedDocumentIds = _dbContext.Set<SubmissionDocument>()
            .Where(sd =>
                EF.Property<SubmissionId>(sd, "SubmissionId") == submissionId)
            .Select(sd => sd.ProductDocumentId);

        var rows = await (
            from document in _dbContext.ProductDocuments.AsNoTracking()
            where document.GlobalProductId == globalProductId
                && document.Status == ProductDocumentStatus.Active
                && !attachedDocumentIds.Contains(document.Id)
            join documentType in _dbContext.DocumentTypes
                on document.DocumentTypeId equals documentType.Id
            orderby document.Name
            select new
            {
                document.Id,
                document.Name,
                DocumentType = documentType.Name,
                document.Status,
                CurrentVersionNumber = _dbContext.Set<DocumentVersion>()
                    .Where(v => v.Id == document.CurrentVersionId)
                    .Select(v => (int?)v.VersionNumber)
                    .FirstOrDefault(),
                document.CreatedOnUtc,
            }).ToListAsync(cancellationToken);

        return rows
            .Select(row => new AttachableProductDocument(
                row.Id.Value,
                row.Name,
                row.DocumentType,
                row.CurrentVersionNumber,
                row.Status.ToString(),
                row.CreatedOnUtc))
            .ToList();
    }
}
