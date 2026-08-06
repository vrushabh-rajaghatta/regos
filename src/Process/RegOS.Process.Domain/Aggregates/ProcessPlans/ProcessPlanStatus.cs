namespace RegOS.Process.Domain.Aggregates.ProcessPlans;

/// <summary>
/// Where a plan stands. <b>Two states in S003, and that is deliberate.</b>
/// </summary>
/// <remarks>
/// A plan is instantiated as a <see cref="Draft"/> — a schedule somebody is
/// still deciding about — and becomes <see cref="Active"/> when the team commits
/// to working it. That pairs with the objective's own <c>Proposed → Active</c>,
/// and an organisation working through execution options before formally
/// committing to the goal is the normal case: a <c>Proposed</c> objective may
/// carry a <c>Draft</c> plan.
/// <para>
/// <b><c>Completed</c> and <c>Cancelled</c> are deliberately absent until
/// S004.</b> Both are statements about execution finishing, and only the story
/// that models execution has enough information to define them correctly.
/// Inventing them here would be inventing lifecycle transitions before knowing
/// what execution means.
/// </para>
/// </remarks>
public enum ProcessPlanStatus
{
    Draft = 1,
    Active = 2
}
