using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Queries.GetSubmissionSnapshot;

/// <summary>
/// Returns the published dossier for a submission by projecting directly from EF —
/// no aggregate is materialized, no repository is used. Queried by SubmissionId
/// because the business speaks about submissions, not snapshot identifiers.
/// </summary>
public sealed class GetSubmissionSnapshotHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetSubmissionSnapshotHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Projects the submission's snapshot into a read model, or null when the
    /// submission has not been published (so the endpoint can 404).
    /// </summary>
    public async Task<PublishedSubmissionDto?> HandleAsync(
        SubmissionId submissionId,
        CancellationToken cancellationToken)
    {
        // PublishedAt lives on the Submission, not the snapshot, so the read side
        // joins the two. Documents are ordered explicitly — never rely on the
        // database's order. Strongly-typed ids are materialized then unwrapped in
        // memory (their converters have no SQL translation for .Value).
        var projection = await (
            from snapshot in _dbContext.SubmissionSnapshots.AsNoTracking()
            where snapshot.SubmissionId == submissionId
            join submission in _dbContext.Submissions
                on snapshot.SubmissionId equals submission.Id
            select new
            {
                submission.PublishedAt,
                Documents = snapshot.Documents
                    .OrderBy(d => d.DisplayOrder)
                    .Select(d => new { d.DisplayOrder, d.DocumentVersionId })
                    .ToList(),
            }).FirstOrDefaultAsync(cancellationToken);

        if (projection is null)
            return null;

        return new PublishedSubmissionDto(
            submissionId.Value,
            projection.PublishedAt,
            projection.Documents
                .Select(d => new PublishedDocumentDto(
                    d.DisplayOrder,
                    d.DocumentVersionId.Value))
                .ToList());
    }
}
