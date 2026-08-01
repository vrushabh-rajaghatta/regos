using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Product.Application.Queries.GetMedicinalProduct;

/// <summary>
/// Everything about one market, for the surface it now has of its own.
/// </summary>
/// <remarks>
/// Deliberately does not return the registrations held here. They belong to
/// another context, and the page already knows the global product — so it reads
/// them from the Registration slice's own query and filters by this market's
/// id. That keeps the dependency running one way (ADR-039 principle 5).
/// </remarks>
public sealed class GetMedicinalProductHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetMedicinalProductHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MedicinalProductDetailDto?> HandleAsync(
        GetMedicinalProductQuery query,
        CancellationToken cancellationToken)
    {
        var row = await (
            from market in _dbContext.MedicinalProducts.AsNoTracking()
            where market.Id == query.MedicinalProductId
            join product in _dbContext.Products
                on market.GlobalProductId equals product.Id
            join country in _dbContext.Countries
                on market.CountryId equals country.Id
            select new
            {
                market.Id,
                market.GlobalProductId,
                ProductName = product.Name,
                ProductCode = product.Code,
                market.CountryId,
                CountryName = country.Name,
                CountryCode = country.Code,
                market.Status,
                market.StatusDate,
                market.CurrentMarketStatus,
                TradeNames = market.TradeNames
                    .OrderBy(name => name.Name)
                    .Select(name => new
                    {
                        name.Id,
                        name.Language,
                        name.Name,
                    })
                    .ToList(),
                History = market.MarketStatusHistory
                    .Select(entry => new
                    {
                        entry.Id,
                        entry.Status,
                        entry.OccurredOn,
                        entry.RecordedOnUtc,
                        entry.Note,
                    })
                    .ToList(),
            }).FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return null;

        // Chronological by what happened, then by what was learned — two
        // entries can share a business date when a portfolio is migrated, and
        // the order they were recorded in is the tie-break a reader expects.
        // The same ordering the registration detail uses.
        var history = row.History
            .OrderBy(entry => entry.OccurredOn)
            .ThenBy(entry => entry.RecordedOnUtc)
            .ToList();

        return new MedicinalProductDetailDto(
            row.Id.Value,
            row.GlobalProductId.Value,
            row.ProductName.Value,
            row.ProductCode.Value,
            row.CountryId.Value,
            row.CountryName,
            row.CountryCode,
            row.Status.ToString(),
            row.StatusDate,
            row.CurrentMarketStatus.ToString(),
            history
                .Where(entry => entry.Status == Domain.Product.MarketStatus.Launched)
                .Select(entry => (DateOnly?)entry.OccurredOn)
                .FirstOrDefault(),
            [.. row.TradeNames.Select(name => new TradeNameDto(
                name.Id.Value, name.Language.Value, name.Name))],
            [.. history.Select(entry => new MarketStatusEntryDto(
                entry.Id.Value,
                entry.Status.ToString(),
                entry.OccurredOn,
                entry.RecordedOnUtc,
                entry.Note))]);
    }
}
