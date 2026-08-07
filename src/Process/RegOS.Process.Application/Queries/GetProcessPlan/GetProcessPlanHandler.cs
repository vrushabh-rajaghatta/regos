using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Application.Queries.GetProcessPlan;

/// <summary>
/// One plan, its schedule, and whether the playbook has moved on without it.
/// </summary>
/// <remarks>
/// <b>D6's disclosure lives here.</b> A plan whose pinned version has been
/// superseded keeps working exactly as it was — nothing migrates, nothing
/// recalculates — and it <em>says</em> so. That was the half of ADR-035's open
/// re-binding question which was missing rather than deferred.
/// </remarks>
public sealed class GetProcessPlanHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetProcessPlanHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProcessPlanDetails> HandleAsync(
        GetProcessPlanQuery query,
        CancellationToken cancellationToken = default)
    {
        var id = ProcessPlanId.From(query.Id);

        var row = await (
            from plan in _dbContext.ProcessPlans.AsNoTracking()
            where plan.Id == id
            join objective in _dbContext.ProcessObjectives
                on plan.ProcessObjectiveId equals objective.Id
            from definition in _dbContext.ProcessDefinitions
                .Where(d => d.Versions.Any(
                    v => v.Id == plan.ProcessDefinitionVersionId))
            select new
            {
                plan.Id,
                plan.Name,
                plan.CurrentStatus,
                plan.ProcessObjectiveId,
                ObjectiveName = objective.Name,
                plan.ProcessDefinitionVersionId,
                DefinitionName = definition.Name,
                Version = definition.Versions
                    .Where(v => v.Id == plan.ProcessDefinitionVersionId)
                    .Select(v => new { v.VersionNumber, v.Status })
                    .First(),
                plan.AnchorDate,
                History = plan.History.Select(x => x.OccurredOn).ToList(),
                Steps = plan.Steps
                    .Select(step => new
                    {
                        step.Id,
                        step.Code,
                        step.Name,
                        step.Description,
                        step.ParentStepId,
                        step.Order,
                        step.PlannedStartOn,
                        step.PlannedEndOn,
                        Predecessors = step.Predecessors
                            .Select(x => x.PredecessorStepId)
                            .ToList(),
                        step.CurrentStatus,
                        History = step.History
                            .Select(h => new
                            {
                                h.Status,
                                h.OccurredOn,
                                h.RecordedOnUtc,
                                h.Note,
                                h.Id
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("That plan does not exist.");

        var codeOf = row.Steps.ToDictionary(x => x.Id, x => x.Code);

        // The reverse view, composed rather than navigated: Process holds no FK
        // to any of these (ADR-065 D2), so it asks the database instead. Six
        // reads over the whole plan, never one per step — see PlanAttachments.
        var attachedByStep = await PlanAttachments.ForStepsAsync(
            _dbContext,
            row.Steps.Select(x => x.Id).ToList(),
            cancellationToken);

        // Schedule order, then code. Deterministic: two steps may legitimately
        // start on the same day, and a step code carries a unique index per plan,
        // so the pair is total.
        var steps = row.Steps
            .OrderBy(step => step.PlannedStartOn)
            .ThenBy(step => step.Code, StringComparer.Ordinal)
            .Select(step => new PlannedStepDetails(
                step.Id.Value,
                step.Code,
                step.Name,
                step.Description,
                step.ParentStepId?.Value,
                step.Order,
                step.PlannedStartOn,
                step.PlannedEndOn,
                [.. step.Predecessors
                    .Select(x => codeOf.GetValueOrDefault(x, "?"))
                    // Deterministic: a step waits for another at most once
                    // (unique index), and one code per step.
                    .OrderBy(code => code, StringComparer.Ordinal)],
                step.CurrentStatus.ToString(),
                // Derived here exactly as the aggregate derives them — a stored
                // copy could disagree with the entries it summarises.
                step.History
                    .Where(h => h.Status == ProcessStepStatus.InProgress)
                    .Select(h => (DateOnly?)h.OccurredOn)
                    .FirstOrDefault(),
                step.History
                    .Where(h => h.Status is ProcessStepStatus.Complete
                        or ProcessStepStatus.Skipped)
                    .Select(h => (DateOnly?)h.OccurredOn)
                    .FirstOrDefault(),
                step.CurrentStatus is ProcessStepStatus.Complete
                    or ProcessStepStatus.Skipped,
                [.. step.History
                    // Oldest first — a history is read forwards. RecordedOnUtc
                    // breaks a same-day tie, and the entry id makes it total.
                    .OrderBy(h => h.OccurredOn)
                    .ThenBy(h => h.RecordedOnUtc)
                    .ThenBy(h => h.Id.Value)
                    .Select(h => new StepHistoryEntry(
                        h.Status.ToString(),
                        h.OccurredOn,
                        h.RecordedOnUtc,
                        h.Note))],
                [.. attachedByStep.GetValueOrDefault(step.Id, [])
                    // Deterministic: an artefact appears once, and (Kind, Id) is
                    // unique across the two sources.
                    .OrderBy(x => x.Kind, StringComparer.Ordinal)
                    .ThenBy(x => x.Title, StringComparer.Ordinal)
                    .ThenBy(x => x.Id)]))
            .ToList();

        return new ProcessPlanDetails(
            row.Id.Value,
            row.Name,
            row.CurrentStatus.ToString(),
            row.ProcessObjectiveId.Value,
            row.ObjectiveName,
            row.ProcessDefinitionVersionId.Value,
            row.DefinitionName,
            row.Version.VersionNumber,
            row.Version.Status == ProcessDefinitionVersionStatus.Superseded,
            row.AnchorDate,
            row.History.Min(),
            // The plan's own span, derived from its steps rather than stored —
            // a second copy could disagree with the schedule it summarises.
            steps.Count == 0 ? null : steps.Min(x => x.PlannedStartOn),
            steps.Count == 0 ? null : steps.Max(x => x.PlannedEndOn),
            steps);
    }
}
