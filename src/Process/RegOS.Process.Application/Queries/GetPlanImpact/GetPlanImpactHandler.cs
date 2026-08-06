using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Application.Queries.GetPlanImpact;

/// <summary>
/// <em>"Given what has happened, what does it now mean?"</em>
/// </summary>
/// <remarks>
/// <b>The arithmetic lives in the domain and this handler does none of it.</b>
/// <c>PlanImpact</c> takes a neutral <c>ScheduledStep</c> shape precisely so that
/// this read can project straight from <c>RegOSDbContext</c> without loading an
/// aggregate (ADR-016) — and so that a second copy of the walk never appears in
/// the application layer.
/// <para>
/// <b>Nothing here writes.</b> ADR-065 I8: the projection is computed on request
/// and discarded, the plan's own dates never change, and re-running it on the
/// same inputs gives the same answer. <em>The forecast is not the ledger.</em>
/// </para>
/// </remarks>
public sealed class GetPlanImpactHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetPlanImpactHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PlanImpactDetails> HandleAsync(
        GetPlanImpactQuery query,
        CancellationToken cancellationToken = default)
    {
        var id = ProcessPlanId.From(query.PlanId);

        var row = await (
            from plan in _dbContext.ProcessPlans.AsNoTracking()
            where plan.Id == id
            join objective in _dbContext.ProcessObjectives
                on plan.ProcessObjectiveId equals objective.Id
            select new
            {
                plan.Id,
                plan.Name,
                ObjectiveName = objective.Name,
                plan.AnchorDate,
                Steps = plan.Steps
                    .Select(step => new
                    {
                        step.Id,
                        step.Code,
                        step.Name,
                        step.PlannedStartOn,
                        step.PlannedEndOn,
                        step.CurrentStatus,
                        History = step.History
                            .Select(h => new { h.Status, h.OccurredOn })
                            .ToList(),
                        Predecessors = step.Predecessors
                            .Select(x => x.PredecessorStepId)
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("That plan does not exist.");

        // Actual dates are derived from history here exactly as the aggregate
        // derives them — a stored copy could disagree with the entries it
        // summarises.
        var scheduled = row.Steps
            .Select(step => new ScheduledStep(
                step.Id,
                step.Code,
                step.PlannedStartOn,
                step.PlannedEndOn,
                step.History
                    .Where(h => h.Status == ProcessStepStatus.InProgress)
                    .Select(h => (DateOnly?)h.OccurredOn)
                    .FirstOrDefault(),
                step.History
                    .Where(h => h.Status is ProcessStepStatus.Complete
                        or ProcessStepStatus.Skipped)
                    .Select(h => (DateOnly?)h.OccurredOn)
                    .FirstOrDefault(),
                step.CurrentStatus,
                [.. step.Predecessors]))
            .ToList();

        var projection = PlanImpact.Project(
            scheduled, row.AnchorDate, query.AsOf);

        var byId = row.Steps.ToDictionary(step => step.Id);

        // Late means unsettled and past its planned end. A settled step that
        // finished late is history, not a risk — the question here is what still
        // threatens the finish date.
        var late = scheduled
            .Where(step => !step.IsSettled && query.AsOf > step.PlannedEndOn)
            .Select(step => new LateStepImpact(
                step.Id.Value,
                step.Code,
                byId[step.Id].Name,
                step.Status.ToString(),
                query.AsOf.DayNumber - step.PlannedEndOn.DayNumber,
                step.PlannedEndOn,
                projection.Steps[step.Id].ProjectedEndOn,
                [.. PlanImpact.Downstream(scheduled, step.Id)
                    .Select(affectedId => new AffectedStep(
                        affectedId.Value,
                        byId[affectedId].Code,
                        byId[affectedId].Name,
                        byId[affectedId].CurrentStatus.ToString(),
                        byId[affectedId].CurrentStatus
                            is not (ProcessStepStatus.Complete
                                or ProcessStepStatus.Skipped)))
                    // Deterministic: a step code is unique per plan.
                    .OrderBy(affected => affected.Code, StringComparer.Ordinal)]))
            // Worst first, then by code. Deterministic: code is unique per plan.
            .OrderByDescending(step => step.DaysLate)
            .ThenBy(step => step.Code, StringComparer.Ordinal)
            .ToList();

        return new PlanImpactDetails(
            row.Id.Value,
            row.Name,
            row.ObjectiveName,
            query.AsOf,
            projection.PlannedFinishOn,
            projection.ProjectedFinishOn,
            projection.SlipDays,
            late);
    }
}
