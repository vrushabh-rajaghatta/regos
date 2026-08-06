namespace RegOS.Process.Domain.Aggregates.ProcessPlans;

/// <summary>
/// What is happening to one step of a plan.
/// </summary>
/// <remarks>
/// <b>Every transition is a business decision recorded by a user</b>
/// ([ADR-065](../../../../../docs/adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md)
/// D11). A linked submission reaching <c>Transmitted</c>, a meeting being
/// recorded, or every predecessor completing may all <em>suggest</em> a step is
/// done. None of them moves it.
/// <para>
/// <b><see cref="Skipped"/> is reachable from both open states, and that is the
/// domain rather than convenience.</b> A team may decide before starting that a
/// step does not apply, or discover it halfway through. Both are real regulatory
/// outcomes, and neither is a deletion — <em>"we deliberately did not do this,
/// and here is why"</em> is evidence.
/// </para>
/// </remarks>
public enum ProcessStepStatus
{
    NotStarted = 1,
    InProgress = 2,

    /// <summary>Done. Terminal.</summary>
    Complete = 3,

    /// <summary>Deliberately not performed. Terminal, and requires a reason.</summary>
    Skipped = 4
}
