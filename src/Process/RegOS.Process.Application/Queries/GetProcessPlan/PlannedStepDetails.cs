namespace RegOS.Process.Application.Queries.GetProcessPlan;

/// <summary>
/// One scheduled step. <b>Both dates are inclusive</b> — a five-day step
/// starting on the 1st ends on the 5th.
/// </summary>
/// <param name="Predecessors">
/// What this waits for, by code. The plan carries its own copy of the graph, so
/// reading it never touches the playbook it came from.
/// </param>
public sealed record PlannedStepDetails(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid? ParentStepId,
    int Order,
    DateOnly PlannedStartOn,
    DateOnly PlannedEndOn,
    IReadOnlyList<string> Predecessors);
