namespace RegOS.Process.Domain.Aggregates.ProcessObjectives;

/// <summary>
/// Where an objective stands. <b>Four states, and two of them are terminal.</b>
/// </summary>
/// <remarks>
/// <b>This is a lifecycle of intent, not of execution.</b> A plan slipping does
/// not move an objective; abandoning the goal does. That separation is
/// [ADR-065 decision 3](../../../../../docs/adr/ADR-065-regulatory-process-is-an-optional-bounded-context.md)
/// made observable — if these ever start tracking what the steps are doing, the
/// two objects have collapsed into one and the decision needs revisiting.
/// <para>
/// <b>No <c>OnHold</c>.</b> Pausing is a real thing that happens to objectives,
/// and nothing has asked for it — a paused objective today is an
/// <see cref="Active"/> one whose plan has stopped moving, which is visible.
/// Add it when somebody needs to report on it, not before.
/// </para>
/// </remarks>
public enum ProcessObjectiveStatus
{
    /// <summary>Stated, not yet committed to. Portfolio planning lives here.</summary>
    Proposed = 1,

    /// <summary>We are working towards it.</summary>
    Active = 2,

    /// <summary>We got what we were after.</summary>
    Achieved = 3,

    /// <summary>We decided not to. Terminal, and never deleted (ES-018).</summary>
    Abandoned = 4
}
