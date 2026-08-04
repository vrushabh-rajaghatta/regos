using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Labeling.Application.Queries.ListGlobalLabelVersions;

public sealed class ListGlobalLabelVersionsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListGlobalLabelVersionsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// Starts at the label, not at the versions — the filtered root is the only
    /// thing standing between this read and every other tenant's issues.
    /// </remarks>
    public async Task<IReadOnlyList<GlobalLabelVersionSummary>> HandleAsync(
        ListGlobalLabelVersionsQuery query,
        CancellationToken cancellationToken)
    {
        return await _dbContext.GlobalLabels
            .AsNoTracking()
            .Where(x => x.Id == query.GlobalLabelId)
            .SelectMany(x => x.Versions)
            .OrderByDescending(x => x.VersionNumber)
            .Select(version => new GlobalLabelVersionSummary(
                version.Id.Value,
                version.VersionNumber,
                version.Status.ToString(),
                version.ContentId == null
                    ? (Guid?)null
                    : version.ContentId.Value.Value,
                version.ChangeSummary,
                version.EffectiveFrom,
                version.EffectiveTo,
                version.PublishedOnUtc))
            .ToListAsync(cancellationToken);
    }
}
