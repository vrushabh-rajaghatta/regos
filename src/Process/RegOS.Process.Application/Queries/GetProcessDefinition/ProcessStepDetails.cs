namespace RegOS.Process.Application.Queries.GetProcessDefinition;

/// <summary>
/// One authored step. <b>It has no dates</b> — a definition describes work, and
/// dates exist only once a plan is instantiated from it (ADR-065 decision 4).
/// </summary>
/// <param name="OffsetDays">
/// Days after the last predecessor finishes, or after the plan's anchor when
/// there are none.
/// </param>
/// <param name="Predecessors">
/// What this step waits for, by code — the code rather than the id, because a
/// reader of a playbook recognises <c>PRE-IND-MTG</c> and not a guid.
/// </param>
public sealed record ProcessStepDetails(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid? ParentStepId,
    int Order,
    int OffsetDays,
    int DurationDays,
    IReadOnlyList<string> Predecessors);
