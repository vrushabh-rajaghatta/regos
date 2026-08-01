namespace RegOS.Interaction.Domain.Correspondence;

/// <summary>
/// Where a question stands.
/// </summary>
/// <remarks>
/// <b>Three states, chosen by actor rather than by moment.</b> <see cref="Responded"/>
/// answers <em>"have we replied?"</em> and is entirely under our control;
/// <see cref="Resolved"/> answers <em>"has the authority accepted that reply?"</em>
/// and is not. Collapsing them would lose the weeks between, which is exactly
/// the period a regulatory team is anxious about.
/// <para>
/// <b>No transition table</b>, unlike <c>RegistrationLifecycle</c>. The
/// progression here is our own operational one; nothing in it is a fork an
/// authority chooses. <c>HaMeeting</c> (S005) is the one that will need a table,
/// because <em>granted or declined</em> genuinely is the authority's decision —
/// the graph matters there, and here it does not (ADR-039 decision 6).
/// </para>
/// <para>
/// <see cref="Open"/> is reused from <c>Commitment</c>'s planned vocabulary and
/// means one thing in both: <em>raised, not yet discharged</em>. Reusing a word
/// for one concept across tiers preserves the vocabulary; reusing it for two
/// concepts is what ADR-039's rule forbids.
/// </para>
/// </remarks>
public enum HaQuestionStatus
{
    /// <summary>Raised, and we have not replied.</summary>
    Open = 1,

    /// <summary>We have replied. The authority has not yet accepted it.</summary>
    Responded = 2,

    /// <summary>The authority is satisfied. Nothing further is owed.</summary>
    Resolved = 3
}
