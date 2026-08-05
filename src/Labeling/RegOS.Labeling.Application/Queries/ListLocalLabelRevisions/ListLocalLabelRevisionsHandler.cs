using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Labeling.Application.Queries.ListLocalLabelRevisions;

public sealed class ListLocalLabelRevisionsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListLocalLabelRevisionsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// Starts at the label, not at the revisions — the filtered root is the only
    /// thing between this read and every other tenant's approved labelling.
    /// <para>
    /// The core version number is joined from <c>GlobalLabels</c> rather than
    /// stored beside the id: a number is a fact about that version, and copying
    /// it here would let the two disagree.
    /// </para>
    /// </remarks>
    public async Task<IReadOnlyList<LocalLabelRevisionSummary>> HandleAsync(
        ListLocalLabelRevisionsQuery query,
        CancellationToken cancellationToken)
    {
        var coreVersions = _dbContext.GlobalLabels
            .AsNoTracking()
            .SelectMany(x => x.Versions);

        return await _dbContext.LocalLabels
            .AsNoTracking()
            .Where(x => x.Id == query.LocalLabelId)
            .SelectMany(x => x.Revisions)
            .OrderByDescending(x => x.RevisionNumber)
            .ThenBy(x => x.Id)
            .Select(revision => new LocalLabelRevisionSummary(
                revision.Id.Value,
                revision.RevisionNumber,
                revision.Status.ToString(),
                revision.ContentId == null
                    ? (Guid?)null
                    : revision.ContentId.Value.Value,
                revision.DerivedFromGlobalLabelVersionId == null
                    ? (Guid?)null
                    : revision.DerivedFromGlobalLabelVersionId.Value,
                coreVersions
                    .Where(v => v.Id == revision.DerivedFromGlobalLabelVersionId)
                    .Select(v => (int?)v.VersionNumber)
                    .FirstOrDefault(),
                revision.DataCarrierCode,
                revision.ChangeSummary,
                revision.ApprovedOn,
                revision.EffectiveFrom,
                revision.EffectiveTo))
            .ToListAsync(cancellationToken);
    }
}
