namespace RegOS.Process.Application.Queries.ListNextSteps;

/// <param name="IsReady">
/// Every predecessor is settled. <b>Derived, and it says "ready" — never
/// "done"</b> (ADR-065 D11). Nothing here transitions a step; a person does.
/// </param>
/// <param name="DaysLate">
/// Days past the planned end, or null when it is not late. Judged against the
/// query's <c>AsOf</c>, never a clock.
/// </param>
public sealed record NextStepItem(
    Guid PlanId,
    string PlanName,
    Guid StepId,
    string Code,
    string Name,
    string Status,
    DateOnly PlannedStartOn,
    DateOnly PlannedEndOn,
    bool IsReady,
    IReadOnlyList<string> WaitingOn,
    int? DaysLate,
    string ObjectiveName,
    string CountryCode);
