using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.ReferenceData.Application.Queries.Geography.ListCountries;

public sealed class ListCountriesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListCountriesHandler(
        RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CountryDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Countries
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CountryDto(
                c.Id,
                c.Code,
                c.IsoAlpha3Code,
                c.Name,
                c.IsoName,
                // Ordered by code rather than left to Postgres, the call every
                // owned collection in this codebase makes.
                c.Regions.OrderBy(r => r.Code).Select(r => r.Code).ToList()))
            .ToListAsync(cancellationToken);
    }
}
