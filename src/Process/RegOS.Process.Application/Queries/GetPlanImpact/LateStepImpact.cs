namespace RegOS.Process.Application.Queries.GetPlanImpact;

public sealed record LateStepImpact(
    Guid StepId,
    string Code,
    string Name,
    string Status,
    int DaysLate,
    DateOnly PlannedEndOn,
    DateOnly ProjectedEndOn,
    IReadOnlyList<AffectedStep> Affected);
