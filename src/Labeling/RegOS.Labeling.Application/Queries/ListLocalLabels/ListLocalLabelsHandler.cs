using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.Persistence;

namespace RegOS.Labeling.Application.Queries.ListLocalLabels;

public sealed class ListLocalLabelsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListLocalLabelsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// Starts at <c>LocalLabels</c> and reaches revisions through it: a revision
    /// carries no <c>TenantId</c> and has no filter of its own, so a read that
    /// began at the revision table would cross every tenant.
    /// </remarks>
    public async Task<IReadOnlyList<LocalLabelSummary>> HandleAsync(
        ListLocalLabelsQuery query,
        CancellationToken cancellationToken)
    {
        return await _dbContext.LocalLabels
            .AsNoTracking()
            .Where(x => x.MedicinalProductId == query.MedicinalProductId)
            .OrderBy(x => x.CreatedOnUtc)
            .Select(label => new LocalLabelSummary(
                label.Id.Value,
                label.LabelType.Code,
                label.LabelType.Display,
                label.LabelType.System,
                label.Language.Value,

                label.Revisions
                    .Where(r => r.Status == LocalLabelRevisionStatus.InForce)
                    .Select(r => (int?)r.RevisionNumber)
                    .FirstOrDefault(),

                label.Revisions
                    .Where(r => r.Status == LocalLabelRevisionStatus.InForce)
                    .Select(r => r.ApprovedOn)
                    .FirstOrDefault(),

                label.Revisions
                    .Where(r => r.Status == LocalLabelRevisionStatus.InForce)
                    .Select(r => r.EffectiveFrom)
                    .FirstOrDefault(),

                label.Revisions
                    .Where(r => r.Status == LocalLabelRevisionStatus.Draft)
                    .Select(r => (Guid?)r.Id.Value)
                    .FirstOrDefault(),

                label.Revisions
                    .Where(r => r.Status == LocalLabelRevisionStatus.Draft)
                    .Select(r => (int?)r.RevisionNumber)
                    .FirstOrDefault(),

                label.Revisions.Count))
            .ToListAsync(cancellationToken);
    }
}
