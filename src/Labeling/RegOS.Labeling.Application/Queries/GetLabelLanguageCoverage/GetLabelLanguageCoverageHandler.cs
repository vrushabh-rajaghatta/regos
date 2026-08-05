using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Labeling.Application.Queries.GetLabelLanguageCoverage;

public sealed class GetLabelLanguageCoverageHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetLabelLanguageCoverageHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// <b>Two reads, deliberately not one join.</b> The expected set is a fact
    /// about the world and the recorded set is the tenant's — only the second
    /// passes through a tenant filter (ADR-031), and joining them would hide
    /// which half is which.
    /// </remarks>
    public async Task<LabelLanguageCoverage> HandleAsync(
        GetLabelLanguageCoverageQuery query,
        CancellationToken cancellationToken)
    {
        var expected = await _dbContext.MedicinalProducts
            .AsNoTracking()
            .Where(market => market.Id == query.MedicinalProductId)
            .Join(
                _dbContext.Countries,
                market => market.CountryId,
                country => country.Id,
                (_, country) => country)
            .SelectMany(country => country.Languages)
            .Select(language => language.Value)
            .ToListAsync(cancellationToken);

        var recorded = await _dbContext.LocalLabels
            .AsNoTracking()
            .Where(label => label.MedicinalProductId == query.MedicinalProductId)
            .Select(label => label.Language.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Ordered so the screen does not reshuffle between loads, and compared
        // in memory because both sets are tiny and the comparison is the point
        // rather than the query.
        expected.Sort(StringComparer.Ordinal);
        recorded.Sort(StringComparer.Ordinal);

        return new LabelLanguageCoverage(
            expected,
            recorded,
            [.. expected.Where(language => !recorded.Contains(language))]);
    }
}
