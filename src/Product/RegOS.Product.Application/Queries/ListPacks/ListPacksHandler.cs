using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Product.Application.Queries.ListPacks;

public sealed class ListPacksHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListPacksHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// Starts at the filtered <c>PackagedProducts</c> root. The marketing status
    /// history carries no <c>TenantId</c> and is reachable only through it.
    /// </remarks>
    public async Task<IReadOnlyList<PackSummary>> HandleAsync(
        ListPacksQuery query,
        CancellationToken cancellationToken)
    {
        var rows = await _dbContext.PackagedProducts
            .AsNoTracking()
            .Where(x => x.MedicinalProductId == query.MedicinalProductId)
            .OrderBy(x => x.CreatedOnUtc)
            .Select(pack => new
            {
                pack.Id,
                pack.Description,
                pack.PackSizeQuantity,
                Unit = pack.PackSizeUnit,
                pack.PackCode,
                pack.CurrentMarketingStatus,

                // The date the status in force took effect. Max, not last —
                // nothing orders what the database hands back.
                Since = pack.MarketingStatusHistory.Max(x => x.OccurredOn),

                History = pack.MarketingStatusHistory
                    .OrderByDescending(entry => entry.OccurredOn)
                    .Select(entry => new
                    {
                        entry.Id,
                        entry.Status,
                        entry.OccurredOn,
                        entry.RecordedOnUtc,
                        entry.Note,
                    })
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        // Strongly-typed ids are materialised then unwrapped in memory: their
        // converters have no SQL translation for .Value.
        return rows
            .Select(row => new PackSummary(
                row.Id.Value,
                row.Description,
                row.PackSizeQuantity,
                row.Unit?.Code,
                row.Unit?.Display,
                row.Unit?.System,
                row.PackCode,
                row.CurrentMarketingStatus.ToString(),
                row.Since,
                [.. row.History.Select(entry => new PackMarketingStatusSummary(
                    entry.Id.Value,
                    entry.Status.ToString(),
                    entry.OccurredOn,
                    entry.RecordedOnUtc,
                    entry.Note))]))
            .ToList();
    }
}
