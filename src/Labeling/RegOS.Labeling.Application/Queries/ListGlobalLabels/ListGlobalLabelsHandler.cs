using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.Persistence;

namespace RegOS.Labeling.Application.Queries.ListGlobalLabels;

public sealed class ListGlobalLabelsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListGlobalLabelsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// The walk starts at <c>GlobalLabels</c> and reaches versions through it.
    /// That is not a stylistic choice: a version carries no <c>TenantId</c> and
    /// has no query filter of its own, so a read that began at the version table
    /// would cross every tenant (ADR-059, and the isolation lesson EPIC-010a's
    /// capstone paid for).
    /// </remarks>
    public async Task<IReadOnlyList<GlobalLabelSummary>> HandleAsync(
        ListGlobalLabelsQuery query,
        CancellationToken cancellationToken)
    {
        return await _dbContext.GlobalLabels
            .AsNoTracking()
            .Where(x => x.GlobalProductId == query.GlobalProductId)
            .OrderBy(x => x.CreatedOnUtc)
            .Select(label => new GlobalLabelSummary(
                label.Id.Value,
                label.Name,
                label.LabelType.Code,
                label.LabelType.Display,
                label.LabelType.System,

                label.Versions
                    .Where(v => v.Status == GlobalLabelVersionStatus.InForce)
                    .Select(v => (int?)v.VersionNumber)
                    .FirstOrDefault(),

                label.Versions
                    .Where(v => v.Status == GlobalLabelVersionStatus.InForce)
                    .Select(v => v.EffectiveFrom)
                    .FirstOrDefault(),

                label.Versions
                    .Where(v => v.Status == GlobalLabelVersionStatus.Draft)
                    .Select(v => (Guid?)v.Id.Value)
                    .FirstOrDefault(),

                label.Versions
                    .Where(v => v.Status == GlobalLabelVersionStatus.Draft)
                    .Select(v => (int?)v.VersionNumber)
                    .FirstOrDefault(),

                label.Versions.Count))
            .ToListAsync(cancellationToken);
    }
}
