using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Application.Queries.ListIndications;
using RegOS.Persistence;

namespace RegOS.Labeling.Application.Queries.ListUndesirableEffects;

public sealed class ListUndesirableEffectsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListUndesirableEffectsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// Starts at the filtered root; populations carry no <c>TenantId</c>.
    /// <para>
    /// <c>PopulationSummary</c> is reused from <c>ListIndications</c> rather
    /// than copied: it is the wire shape of one type mapped three times, and a
    /// second identical record would be duplication with no reader.
    /// </para>
    /// </remarks>
    public async Task<IReadOnlyList<UndesirableEffectSummary>> HandleAsync(
        ListUndesirableEffectsQuery query,
        CancellationToken cancellationToken)
    {
        return await _dbContext.UndesirableEffects
            .AsNoTracking()
            .Where(x => x.MedicinalProductId == query.MedicinalProductId)
            .OrderBy(x => x.CreatedOnUtc)
            .Select(statement => new UndesirableEffectSummary(
                statement.Id.Value,
                statement.Effect.Code,
                statement.Effect.Display,
                statement.Effect.System,
                statement.LabelText,
                statement.Frequency == null ? null : statement.Frequency.Code,
                statement.Frequency == null
                    ? null
                    : statement.Frequency.Display,
                statement.Populations
                    .Select(population => new PopulationSummary(
                        population.Id.Value,
                        population.AgeLow,
                        population.AgeHigh,
                        population.AgeUnit == null
                            ? null
                            : population.AgeUnit.Code,
                        population.AgeUnit == null
                            ? null
                            : population.AgeUnit.Display,
                        population.Gender.Code,
                        population.Gender.Display,
                        population.PhysiologicalCondition == null
                            ? null
                            : population.PhysiologicalCondition.Code,
                        population.PhysiologicalCondition == null
                            ? null
                            : population.PhysiologicalCondition.Display,
                        population.Description))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }
}
