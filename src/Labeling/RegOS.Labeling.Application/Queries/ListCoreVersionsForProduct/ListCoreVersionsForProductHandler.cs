using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.Persistence;

namespace RegOS.Labeling.Application.Queries.ListCoreVersionsForProduct;

public sealed class ListCoreVersionsForProductHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListCoreVersionsForProductHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// Starts at the filtered <c>GlobalLabels</c> root — a version has no
    /// <c>TenantId</c> and no filter of its own.
    /// <para>
    /// Drafts are excluded. A market cannot be written from a core version the
    /// company has not issued, and offering one would let a Japanese revision
    /// claim a lineage that does not exist yet.
    /// </para>
    /// </remarks>
    public async Task<IReadOnlyList<CoreVersionOption>> HandleAsync(
        ListCoreVersionsForProductQuery query,
        CancellationToken cancellationToken)
    {
        return await _dbContext.GlobalLabels
            .AsNoTracking()
            .Where(x => x.GlobalProductId == query.GlobalProductId)
            .SelectMany(
                label => label.Versions,
                (label, version) => new { label, version })
            .Where(x => x.version.Status != GlobalLabelVersionStatus.Draft)
            .OrderByDescending(x => x.version.VersionNumber)
            .Select(x => new CoreVersionOption(
                x.version.Id.Value,
                x.label.Id.Value,
                x.label.Name,
                x.version.VersionNumber,
                x.version.Status.ToString(),
                x.version.EffectiveFrom))
            .ToListAsync(cancellationToken);
    }
}
