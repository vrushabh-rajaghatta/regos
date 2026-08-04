namespace RegOS.Labeling.Domain.Aggregates.LocalLabels;

/// <summary>
/// Where one revision of a market's label sits in its life.
/// </summary>
/// <remarks>
/// <b>Deliberately not the authority's process.</b> Submitted, under review,
/// questions outstanding, approved-with-conditions — those are EPIC-008's, and
/// this epic records the dated facts that process produces rather than the
/// process itself. The three states below are what is true of the
/// <em>document</em>, which is a different question from where the dossier has
/// got to.
/// <para>
/// The same three words as <c>GlobalLabelVersionStatus</c>, and not the same
/// enum: the two lifecycles are unrelated (EPIC-018 D1), and merging them would
/// mean a rule added to one silently reaching the other.
/// </para>
/// </remarks>
public enum LocalLabelRevisionStatus
{
    /// <summary>Being prepared. The only state in which anything can change.</summary>
    Draft = 0,

    /// <summary>Approved and current in this market. At most one.</summary>
    InForce = 1,

    /// <summary>
    /// Was in force, and a later revision replaced it. Retained — an approved
    /// labelling document is a controlled record, and overwriting one is a
    /// governance failure rather than an edit.
    /// </summary>
    Superseded = 2
}
