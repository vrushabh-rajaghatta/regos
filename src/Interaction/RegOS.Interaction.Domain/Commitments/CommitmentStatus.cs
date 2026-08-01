namespace RegOS.Interaction.Domain.Commitments;

/// <summary>
/// Where a commitment stands.
/// </summary>
/// <remarks>
/// <b>There is deliberately no <c>Failed</c> or <c>Missed</c>, and that absence
/// is the decision.</b> A commitment we did not do is <see cref="Open"/> and
/// past its date — <em>overdue is derived, never a status</em>. RegOS does not
/// get to record that we failed; the authority records that, in a letter, which
/// is correspondence. A failure status would put someone else's judgement in
/// our database.
/// <para>
/// <see cref="Fulfilled"/> and <see cref="Waived"/> are separated by actor, the
/// same test that separated <c>Responded</c> from <c>Resolved</c>: fulfilling is
/// something we perform, waiving is something the authority does. <em>Waived</em>
/// rather than <em>Cancelled</em> because a cancelled meeting (it did not
/// happen) and a released obligation are two concepts, and the due view shows
/// both at once — ADR-039's vocabulary rule.
/// </para>
/// <para>
/// <see cref="Open"/> is the same word <c>HaQuestionStatus</c> uses and means
/// the same thing in both: <em>raised, not yet discharged</em>. One concept,
/// two tiers, which the vocabulary rule permits.
/// </para>
/// <para>
/// No transition table. The progression is our own operational one; nothing in
/// it is a fork an authority chooses (ADR-039 decision 6).
/// </para>
/// </remarks>
public enum CommitmentStatus
{
    /// <summary>Promised, not started.</summary>
    Open = 1,

    /// <summary>Being worked on.</summary>
    InProgress = 2,

    /// <summary>We did it.</summary>
    Fulfilled = 3,

    /// <summary>The authority released us from it.</summary>
    Waived = 4
}
