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
            .ThenBy(x => x.Id)
            .Select(pack => new
            {
                pack.Id,
                pack.Description,
                pack.PackSizeQuantity,
                Unit = pack.PackSizeUnit,
                pack.PackCode,
                pack.CurrentMarketingStatus,

                LegalStatus = pack.LegalStatusOfSupply,

                // ShelfLife is a required owned reference, so it is never null
                // here — a pack nobody has spoken about carries the empty
                // statement rather than a missing one.
                ShelfLifeValue = pack.ShelfLife.Value,
                ShelfLifeUnit = pack.ShelfLife.Unit,
                ShelfLifeText = pack.ShelfLife.Text,
                StorageConditions = pack.ShelfLife.StorageConditions
                    .Select(condition => new
                    {
                        condition.Code,
                        condition.Display,
                    })
                    .ToList(),

                // The second collection behind that same required navigation —
                // what the period was demonstrated under, not how the pack is
                // kept.
                TestedAt = pack.ShelfLife.TestedAt
                    .Select(condition => new
                    {
                        condition.Code,
                        condition.Display,
                    })
                    .ToList(),

                // The date the status in force took effect. Max, not last —
                // nothing orders what the database hands back.
                Since = pack.MarketingStatusHistory.Max(x => x.OccurredOn),

                History = pack.MarketingStatusHistory
                    .OrderByDescending(entry => entry.OccurredOn)
                    .ThenBy(entry => entry.Id)
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
                row.LegalStatus?.Code,
                row.LegalStatus?.Display,
                row.ShelfLifeValue,
                row.ShelfLifeUnit?.Code,
                row.ShelfLifeUnit?.Display,
                row.ShelfLifeText,
                [.. row.StorageConditions.Select(condition =>
                    new PackStorageConditionSummary(
                        condition.Code, condition.Display))],
                [.. row.TestedAt.Select(condition =>
                    new PackTestedAtSummary(
                        condition.Code, condition.Display))],
                [.. row.History.Select(entry => new PackMarketingStatusSummary(
                    entry.Id.Value,
                    entry.Status.ToString(),
                    entry.OccurredOn,
                    entry.RecordedOnUtc,
                    entry.Note))]))
            .ToList();
    }
}
