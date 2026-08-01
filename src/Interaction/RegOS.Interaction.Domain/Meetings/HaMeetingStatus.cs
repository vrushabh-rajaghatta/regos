namespace RegOS.Interaction.Domain.Meetings;

/// <summary>
/// Where a meeting stands.
/// </summary>
/// <remarks>
/// <b>The only lifecycle in this context with a transition table</b>, and the
/// reason is narrow: <see cref="Requested"/> → <see cref="Granted"/> or
/// <see cref="Declined"/> is a fork <em>the authority</em> chooses. Every other
/// status graph in EPIC-006 records our own operational progression, where a
/// table would encode one company's habits as law (ADR-039 decision 6).
/// <para>
/// <b>The table models authority decisions, not our workflow.</b> That is the
/// line to hold if someone later proposes adding <em>Minutes Uploaded</em> or
/// <em>Attendees Confirmed</em> — those are things we do, and they are not
/// statuses of a meeting at all.
/// </para>
/// </remarks>
public enum HaMeetingStatus
{
    /// <summary>We asked for it. The authority has not answered.</summary>
    Requested = 1,

    /// <summary>The authority agreed to meet — or called the meeting itself.</summary>
    Granted = 2,

    /// <summary>The authority refused. Terminal.</summary>
    Declined = 3,

    /// <summary>It happened. Terminal.</summary>
    Held = 4,

    /// <summary>It was called off before happening. Terminal.</summary>
    Cancelled = 5
}
