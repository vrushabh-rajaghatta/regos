using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.MasterData.Application.Queries.Geography.ListCountries;

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
                c.Name))
            .ToListAsync(cancellationToken);
    }
}
