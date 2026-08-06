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
/// <b><see cref="Completed"/> and <see cref="Cancelled"/> arrived with S004</b>,
/// once execution existed to give them meaning. Inventing them at S003 would
/// have been inventing lifecycle transitions before knowing what execution was.
/// </para>
/// <para>
/// <b>Completing a plan does not require every step to be <c>Complete</c></b>
/// (ADR-065 D11). Steps may legitimately be <c>Skipped</c>, and the real
/// condition is <em>no further execution is expected</em> — a judgement a person
/// makes, not a count the system takes.
/// </para>
/// </remarks>
public enum ProcessPlanStatus
{
    Draft = 1,
    Active = 2,

    /// <summary>Nothing further is expected of it. Terminal.</summary>
    Completed = 3,

    /// <summary>Abandoned before finishing. Terminal, and never deleted.</summary>
    Cancelled = 4
}
