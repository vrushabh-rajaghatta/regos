namespace RegOS.Process.Application.Queries.GetPlanImpact;

/// <summary>
/// <b>What today's facts imply, if nothing changes.</b>
/// </summary>
/// <param name="ProjectedFinishOn">
/// <b>Not the plan's finish date.</b> The plan still says
/// <see cref="PlannedFinishOn"/> and always will — this is an analysis, computed
/// on request and discarded (ADR-065 I8). Every surface that shows it must label
/// it as a projection.
/// </param>
/// <param name="SlipDays">
/// How much later the plan now finishes. <b>Zero when the delay fell inside
/// somebody's slack</b>, which is the case a naive "the step is nine days late,
/// so the plan is" gets wrong — and gets wrong on the one playbook RegOS ships.
/// </param>
public sealed record PlanImpactDetails(
    Guid PlanId,
    string PlanName,
    string ObjectiveName,
    DateOnly AsOf,
    DateOnly? PlannedFinishOn,
    DateOnly? ProjectedFinishOn,
    int SlipDays,
    IReadOnlyList<LateStepImpact> LateSteps);
