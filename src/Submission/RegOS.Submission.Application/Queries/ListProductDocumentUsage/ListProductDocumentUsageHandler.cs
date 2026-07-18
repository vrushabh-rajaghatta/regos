using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ProductDocument.Domain.Entities;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Queries.ListProductDocumentUsage;

/// <summary>
/// "Where is this document used?" — every submission that references the
/// Product Document, with the version each one pinned. Lives in the Submission
/// application layer because Submission owns the attachment data; the Product
/// Document workspace merely consumes this read model.
/// </summary>
public sealed class ListProductDocumentUsageHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListProductDocumentUsageHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SubmissionDocumentUsage>> HandleAsync(
        ProductDocumentId productDocumentId,
        CancellationToken cancellationToken)
    {
        // "SubmissionId" is the attachment's shadow FK to its owning
        // submission. Strongly-typed ids are materialized then unwrapped in
        // memory (no SQL translation for .Value).
        var rows = await (
            from usage in _dbContext.Set<SubmissionDocument>().AsNoTracking()
            where usage.ProductDocumentId == productDocumentId
            join submission in _dbContext.Submissions
                on EF.Property<SubmissionId>(usage, "SubmissionId")
                equals submission.Id
            join application in _dbContext.RegulatoryApplications
                on submission.ApplicationId equals application.Id
            join authority in _dbContext.Authorities
                on application.AuthorityId equals authority.Id
            join submissionType in _dbContext.SubmissionTypes
                on submission.SubmissionTypeId equals submissionType.Id
            join version in _dbContext.Set<DocumentVersion>()
                on usage.DocumentVersionId equals version.Id
            orderby usage.AttachedAt descending
            select new
            {
                submission.Id,
                submission.ApplicationId,
                SubmissionTitle = submission.Title,
                SubmissionType = submissionType.Name,
                Authority = authority.Name,
                version.VersionNumber,
                usage.AttachedAt,
            }).ToListAsync(cancellationToken);

        return rows
            .Select(row => new SubmissionDocumentUsage(
                row.Id.Value,
                row.ApplicationId.Value,
                row.SubmissionTitle,
                row.SubmissionType,
                row.Authority,
                row.VersionNumber,
                row.AttachedAt))
            .ToList();
    }
}
