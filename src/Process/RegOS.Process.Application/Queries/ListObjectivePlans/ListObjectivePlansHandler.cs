using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessDefinitions;

namespace RegOS.Process.Application.Queries.ListObjectivePlans;

public sealed class ListObjectivePlansHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListObjectivePlansHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ObjectivePlanSummary>> HandleAsync(
        ListObjectivePlansQuery query,
        CancellationToken cancellationToken = default)
    {
        var rows = await (
            from plan in _dbContext.ProcessPlans.AsNoTracking()
            where plan.ProcessObjectiveId == query.ProcessObjectiveId
            from definition in _dbContext.ProcessDefinitions
                .Where(d => d.Versions.Any(
                    v => v.Id == plan.ProcessDefinitionVersionId))
            // Newest attempt first. Deterministic: the anchor date can tie
            // between two plans drawn up for the same start, and the plan id
            // makes the pair total.
            orderby plan.AnchorDate descending, plan.Id
            select new
            {
                plan.Id,
                plan.Name,
                plan.CurrentStatus,
                DefinitionName = definition.Name,
                Version = definition.Versions
                    .Where(v => v.Id == plan.ProcessDefinitionVersionId)
                    .Select(v => new { v.VersionNumber, v.Status })
                    .First(),
                plan.AnchorDate,
                Start = plan.Steps.Min(s => (DateOnly?)s.PlannedStartOn),
                End = plan.Steps.Max(s => (DateOnly?)s.PlannedEndOn),
                StepCount = plan.Steps.Count
            })
            .ToListAsync(cancellationToken);

        return
        [
            .. rows.Select(row => new ObjectivePlanSummary(
                row.Id.Value,
                row.Name,
                row.CurrentStatus.ToString(),
                row.DefinitionName,
                row.Version.VersionNumber,
                row.Version.Status == ProcessDefinitionVersionStatus.Superseded,
                row.AnchorDate,
                row.Start,
                row.End,
                row.StepCount))
        ];
    }
}
