using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ProductDocument.Domain.Entities;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Queries.ListProductDocumentUsage;

/// <summary>
/// "What happened to this document?" — every filing that placed it and every
/// filing that withdrew it, in the order they were made. Lives in the
/// Submission application layer because Submission owns both records; the
/// Product Document workspace merely consumes this read model.
/// </summary>
/// <remarks>
/// <b>Two reads, merged in memory, and deliberately so.</b> Placements and
/// withdrawals are different tables with different shapes — one pins a version,
/// the other records that nothing is there any more — so a SQL union would have
/// to invent columns for both sides. A document's filing history is small, and
/// merging where the shapes are already understood keeps each query honest
/// about what it selects.
/// </remarks>
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
        // "SubmissionId" is the child's shadow FK to its owning submission.
        // Strongly-typed ids are materialized then unwrapped in memory (no SQL
        // translation for .Value).
        var placements = await (
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
            select new
            {
                submission.Id,
                submission.ApplicationId,
                ApplicationName = application.Name,
                SubmissionTitle = submission.Title,
                SubmissionType = submissionType.Name,
                Authority = authority.Name,
                submission.SequenceNumber,
                submission.Status,
                submission.Format,
                usage.Operation,
                version.VersionNumber,
                usage.AttachedAt,
            }).ToListAsync(cancellationToken);

        // The withdrawals. Published-only by construction: deletions are
        // computed at publish and never exist on a draft.
        var withdrawals = await (
            from deletion in _dbContext.Set<SubmissionDeletion>().AsNoTracking()
            where deletion.ProductDocumentId == productDocumentId
            join submission in _dbContext.Submissions
                on EF.Property<SubmissionId>(deletion, "SubmissionId")
                equals submission.Id
            join application in _dbContext.RegulatoryApplications
                on submission.ApplicationId equals application.Id
            join authority in _dbContext.Authorities
                on application.AuthorityId equals authority.Id
            join submissionType in _dbContext.SubmissionTypes
                on submission.SubmissionTypeId equals submissionType.Id
            select new
            {
                submission.Id,
                submission.ApplicationId,
                ApplicationName = application.Name,
                SubmissionTitle = submission.Title,
                SubmissionType = submissionType.Name,
                Authority = authority.Name,
                submission.SequenceNumber,
                submission.Status,
                submission.Format,
            }).ToListAsync(cancellationToken);

        var events = placements
            .Select(row => new SubmissionDocumentUsage(
                row.Id.Value,
                row.ApplicationId.Value,
                row.ApplicationName,
                row.SubmissionTitle,
                row.SubmissionType,
                row.Authority,
                row.SequenceNumber,
                row.Status.ToString(),
                row.Format.ToString(),
                row.Operation?.ToString(),
                row.VersionNumber,
                row.AttachedAt))
            .Concat(withdrawals.Select(row => new SubmissionDocumentUsage(
                row.Id.Value,
                row.ApplicationId.Value,
                row.ApplicationName,
                row.SubmissionTitle,
                row.SubmissionType,
                row.Authority,
                row.SequenceNumber,
                row.Status.ToString(),
                row.Format.ToString(),
                SubmissionContentOperation.Delete.ToString(),
                null,
                null)));

        // Filing order within an application, drafts last: a draft has no
        // number because it has not happened yet.
        return events
            .OrderBy(x => x.ApplicationName, StringComparer.Ordinal)
            .ThenBy(x => x.SequenceNumber is null)
            .ThenBy(x => x.SequenceNumber)
            .ThenBy(x => x.SubmissionTitle, StringComparer.Ordinal)
            .ToList();
    }
}
