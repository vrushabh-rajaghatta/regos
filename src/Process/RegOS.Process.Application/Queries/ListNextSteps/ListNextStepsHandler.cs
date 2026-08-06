using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Process.Application.Queries.ListNextSteps;

/// <summary>
/// The first operational read in the Process context: <em>what should I do
/// today?</em>
/// </summary>
/// <remarks>
/// <b>A sibling of <c>ListDueWork</c>, sharing no code with it</b> (ADR-065 D7).
/// That query answers <em>what does a regulator expect from us?</em> — externally
/// imposed, missing one has compliance consequence. This answers <em>is our own
/// plan slipping?</em> — internally imposed, missing one affects forecasting.
/// They may share rendering later. They may never share behaviour.
/// <para>
/// <b><see cref="NextStepItem.IsReady"/> is the closest this epic comes to
/// derived completion, and it stops well short.</b> It says a step's predecessors
/// are settled, which is a fact about the schedule. It never says the step is
/// done, and nothing here writes anything — D11 holds because the read has no
/// way to violate it.
/// </para>
/// </remarks>
public sealed class ListNextStepsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListNextStepsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<NextStepItem>> HandleAsync(
        ListNextStepsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Active plans only. A draft has not been committed to and a closed one
        // expects nothing further, so neither generates work.
        var rows = await (
            from plan in _dbContext.ProcessPlans.AsNoTracking()
            where plan.CurrentStatus == ProcessPlanStatus.Active
            join objective in _dbContext.ProcessObjectives
                on plan.ProcessObjectiveId equals objective.Id
            join country in _dbContext.Countries
                on objective.CountryId equals country.Id
            select new
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
                ObjectiveName = objective.Name,
                CountryCode = country.Code,
                Steps = plan.Steps
                    .Select(step => new
                    {
                        step.Id,
                        step.Code,
                        step.Name,
                        step.CurrentStatus,
                        step.PlannedStartOn,
                        step.PlannedEndOn,
                        Predecessors = step.Predecessors
                            .Select(x => x.PredecessorStepId)
                            .ToList()
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var items = new List<NextStepItem>();

        foreach (var plan in rows)
        {
            // Settled is either terminal outcome — a skipped predecessor unblocks
            // its successors exactly as a completed one does, because in both
            // cases nothing further is expected of it.
            var settled = plan.Steps
                .Where(step => step.CurrentStatus is ProcessStepStatus.Complete
                    or ProcessStepStatus.Skipped)
                .Select(step => step.Id)
                .ToHashSet();

            var codeOf = plan.Steps.ToDictionary(step => step.Id, step => step.Code);

            foreach (var step in plan.Steps.Where(
                         step => !settled.Contains(step.Id)))
            {
                var waitingOn = step.Predecessors
                    .Where(id => !settled.Contains(id))
                    .Select(id => codeOf.GetValueOrDefault(id, "?"))
                    // Deterministic: a step waits for another at most once
                    // (unique index), and one code per step.
                    .OrderBy(code => code, StringComparer.Ordinal)
                    .ToList();

                var daysLate = query.AsOf > step.PlannedEndOn
                    ? query.AsOf.DayNumber - step.PlannedEndOn.DayNumber
                    : (int?)null;

                items.Add(new NextStepItem(
                    plan.PlanId.Value,
                    plan.PlanName,
                    step.Id.Value,
                    step.Code,
                    step.Name,
                    step.CurrentStatus.ToString(),
                    step.PlannedStartOn,
                    step.PlannedEndOn,
                    waitingOn.Count == 0,
                    waitingOn,
                    daysLate,
                    plan.ObjectiveName,
                    plan.CountryCode));
            }
        }

        // Late first, then by how late, then by what is due soonest. Undated work
        // does not exist here — every step is scheduled — so the only tie left is
        // between two steps due the same day, which the step id settles.
        return
        [
            .. items
                .OrderByDescending(item => item.DaysLate.HasValue)
                .ThenByDescending(item => item.DaysLate)
                .ThenBy(item => item.PlannedEndOn)
                .ThenBy(item => item.StepId)
        ];
    }
}
