using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

using RegistrationAggregate = RegOS.Registration.Domain.Aggregates.Registration.Registration;

namespace RegOS.Registration.Application.Queries.ListRegistrationMarkets;

/// <summary>
/// The way into the market view: the countries this tenant actually holds
/// registrations in.
/// </summary>
/// <remarks>
/// Without this the market workspace has no entry point, because nobody starts
/// by browsing two hundred countries to find the three they are in. The
/// registrations themselves are still fetched per country — this only says
/// which countries are worth opening.
/// </remarks>
public sealed class ListRegistrationMarketsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListRegistrationMarketsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RegistrationMarket>> HandleAsync(
        CancellationToken cancellationToken)
    {
        // Grouped in the database, so a large portfolio does not travel just to
        // be counted. The tenant filter (ADR-031) applies to the registrations,
        // which is what makes a country appear at all.
        var rows = await (
            from registration in _dbContext.Set<RegistrationAggregate>()
                .AsNoTracking()
            join market in _dbContext.MedicinalProducts
                on registration.MedicinalProductId equals market.Id
            join country in _dbContext.Countries
                on market.CountryId equals country.Id
            group country by new
            {
                market.CountryId,
                country.Name,
                country.Code,
            }
            into market
            // BUG-001: the tie-breaker in SQL, where CountryId translates. In
            // memory it has no IComparable and throws on the second market.
            orderby market.Key.CountryId
            select new
            {
                market.Key.CountryId,
                market.Key.Name,
                market.Key.Code,
                Count = market.Count(),
            }).ToListAsync(cancellationToken);

        // Fetched in one pass rather than per row. The grouped query above
        // cannot carry an owned collection through a GROUP BY, and eight
        // countries is not a set worth a join per market.
        var countryIds = rows.Select(row => row.CountryId).ToList();

        var regions = await _dbContext.Countries
            .AsNoTracking()
            .Where(country => countryIds.Contains(country.Id))
            .Select(country => new
            {
                country.Id,
                // Deterministic: a region is a CodedConcept and Code is its
                // identity — an owned collection cannot hold the same code
                // twice for one country, so this ordering is already total.
                Codes = country.Regions
                    .OrderBy(region => region.Code)
                    .Select(region => region.Code)
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        var byCountry = regions.ToDictionary(x => x.Id, x => x.Codes);

        // Deterministic: the source query is ordered by CountryId in SQL and
        // this sort is stable, so equal names keep that order. The tie-break
        // lives there because an id has no IComparable in memory (BUG-001).
        return rows
            .OrderBy(row => row.Name)
            .Select(row => new RegistrationMarket(
                row.CountryId.Value,
                row.Name,
                row.Code,
                row.Count,
                byCountry.TryGetValue(row.CountryId, out var codes)
                    ? codes
                    : []))
            .ToList();
    }
}
