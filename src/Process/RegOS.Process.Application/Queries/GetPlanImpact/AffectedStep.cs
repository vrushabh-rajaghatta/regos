namespace RegOS.Process.Application.Queries.GetPlanImpact;

/// <param name="IsActionable">
/// The step is affected <em>and</em> still open. <b>An affected step that is
/// settled is reported rather than dropped</b> — a skipped step is still
/// downstream of the delay, and a traversal that hid it would answer a different
/// question than the one asked.
/// </param>
public sealed record AffectedStep(
    Guid StepId,
    string Code,
    string Name,
    string Status,
    bool IsActionable);
