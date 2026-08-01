namespace RegOS.Interaction.Domain.Inspections;

/// <summary>
/// Where an inspection stands.
/// </summary>
/// <remarks>
/// <b>No transition table, and the contrast with <c>HaMeeting</c> is the
/// point.</b> A meeting's graph contains a fork the authority <em>chooses</em>
/// — granted or declined — which is a rule. An inspection's progression is a
/// natural sequence: announced, under way, finished. Nobody decides between
/// branches; the authority simply either turns up or does not.
/// <para>
/// So the only rules here are the ones every history has: a concluded
/// inspection does not change, and time does not run backwards. Recording
/// <c>Announced → Completed</c> without the middle state is allowed, because
/// people do not always log the day an inspector walked in.
/// </para>
/// <para>
/// <b>Two honest beginnings</b>, like a meeting: <see cref="Announced"/> when
/// they told us in advance, <see cref="InProgress"/> when they arrived
/// unannounced. Forcing the second through the first would put a notice in the
/// history that was never given.
/// </para>
/// </remarks>
public enum InspectionStatus
{
    /// <summary>They told us it is coming.</summary>
    Announced = 1,

    /// <summary>They are here.</summary>
    InProgress = 2,

    /// <summary>It finished. Terminal.</summary>
    Completed = 3,

    /// <summary>It was called off before happening. Terminal.</summary>
    Cancelled = 4
}
