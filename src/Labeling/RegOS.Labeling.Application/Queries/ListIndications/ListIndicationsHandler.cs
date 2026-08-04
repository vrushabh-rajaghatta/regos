using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Labeling.Application.Queries.ListIndications;

public sealed class ListIndicationsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListIndicationsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// Starts at the filtered <c>Indications</c> root. Populations, therapies
    /// and history carry no <c>TenantId</c> and are reachable only through it.
    /// </remarks>
    public async Task<IReadOnlyList<IndicationSummary>> HandleAsync(
        ListIndicationsQuery query,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Indications
            .AsNoTracking()
            .Where(x => x.MedicinalProductId == query.MedicinalProductId)
            .OrderBy(x => x.CreatedOnUtc)
            .Select(indication => new IndicationSummary(
                indication.Id.Value,
                indication.Condition.Code,
                indication.Condition.Display,
                indication.Condition.System,
                indication.LabelText,
                indication.CurrentStatus.ToString(),

                // The date the decision in force took effect. Max, not last —
                // nothing orders what the database hands back.
                indication.StatusHistory
                    .Max(entry => entry.OccurredOn),

                indication.Populations
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
                    .ToList(),

                indication.OtherTherapies
                    .Select(therapy => new OtherTherapySummary(
                        therapy.Id.Value,
                        therapy.Relationship.Code,
                        therapy.Relationship.Display,
                        therapy.Therapy))
                    .ToList(),

                indication.StatusHistory
                    .OrderByDescending(entry => entry.OccurredOn)
                    .Select(entry => new IndicationDecisionSummary(
                        entry.Id.Value,
                        entry.Status.ToString(),
                        entry.OccurredOn,
                        entry.RecordedOnUtc,
                        entry.Note))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }
}
