using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Domain.Aggregates.Indications;
using RegOS.Persistence;

namespace RegOS.Labeling.Application.Queries.ListMarketsForCondition;

/// <summary>
/// EPIC-018's capstone read — and the story's whole hypothesis.
/// </summary>
/// <remarks>
/// <para>
/// <b>Nothing here is stored for reporting.</b> No projection table, no
/// denormalised summary, no field added to make the question answerable. The
/// condition code was coded in S003 so markets could be compared on it; the
/// status was given a history in S003 because an indication is a regulatory
/// decision. This read is the first one that <em>depends</em> on both being
/// right — which is what makes it a falsifier rather than a demonstration.
/// </para>
/// <para>
/// <b>Starts at the filtered <c>Indications</c> root</b> and joins outward to
/// the market and its country (ADR-031). Both roots carry the tenant filter, so
/// a cross-context join cannot widen what a tenant sees.
/// </para>
/// </remarks>
public sealed class ListMarketsForConditionHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListMarketsForConditionHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <returns>
    /// Null when the global product does not exist — so the endpoint can 404
    /// rather than report "approved nowhere" for a product that was never there.
    /// An empty list is an ordinary answer: this product has no indication for
    /// that condition in any market.
    /// </returns>
    public async Task<IReadOnlyList<ConditionMarketSummary>?> HandleAsync(
        ListMarketsForConditionQuery query,
        CancellationToken cancellationToken)
    {
        var productExists = await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id == query.GlobalProductId, cancellationToken);

        if (!productExists)
            return null;

        // Strongly-typed ids are materialised then unwrapped in memory: their
        // converters have no SQL translation for .Value.
        var rows = await (
            from indication in _dbContext.Indications.AsNoTracking()
            where indication.Condition.Code == query.ConditionCode
            join market in _dbContext.MedicinalProducts.AsNoTracking()
                on indication.MedicinalProductId equals market.Id
            where market.GlobalProductId == query.GlobalProductId
            join country in _dbContext.Countries.AsNoTracking()
                on market.CountryId equals country.Id
            orderby country.Name, indication.Id
            select new
            {
                MarketId = market.Id,
                market.CountryId,
                CountryName = country.Name,
                CountryCode = country.Code,
                IndicationId = indication.Id,
                indication.LabelText,
                indication.CurrentStatus,

                // The date the decision in force took effect. Max, not last —
                // nothing orders what the database hands back.
                Since = indication.StatusHistory.Max(entry => entry.OccurredOn),
            }).ToListAsync(cancellationToken);

        return rows
            .Select(row => new ConditionMarketSummary(
                row.MarketId.Value,
                row.CountryId.Value,
                row.CountryName,
                row.CountryCode,
                row.IndicationId.Value,
                row.LabelText,
                row.CurrentStatus.ToString(),
                row.Since,

                // The domain decides what counts as an approval, not the read.
                Indication.IsAnAuthorisation(row.CurrentStatus)))
            .ToList();
    }
}
