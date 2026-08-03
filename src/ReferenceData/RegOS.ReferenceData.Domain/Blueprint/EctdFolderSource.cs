namespace RegOS.ReferenceData.Domain.Blueprint;

/// <summary>
/// Where a section's eCTD folder name came from.
/// </summary>
/// <remarks>
/// <b>Evidence, derived implementation and RegOS convention must never become
/// indistinguishable in the data.</b> EPIC-007a has kept those three apart in
/// its documents from the first task; the moment RegOS started generating folder
/// names for sections no specification names, they had to be kept apart in the
/// database too — otherwise a value we chose reads exactly like a value ICH
/// published, and the next reader has no way to tell.
/// <para>
/// The blast radius is the point. If ICH restates Appendix 4, every
/// <see cref="IchAppendix4"/> row is suspect and no other row is. If RegOS
/// changes its own convention, the reverse. A single unqualified string would
/// make both questions unanswerable.
/// </para>
/// </remarks>
public enum EctdFolderSource
{
    /// <summary>
    /// ICH eCTD v3.2.2, Appendix 4 — the specification's own directory table.
    /// </summary>
    /// <remarks>
    /// <b>Level 3, not 2a.</b> The appendix says these names are *"not
    /// mandatory, but recommended"* for Modules 2–5, so a package that departs
    /// from them is not thereby invalid. RegOS emits them anyway: deterministic
    /// output and Level 3 comparability are worth more than a freedom nobody
    /// has asked to exercise.
    /// </remarks>
    IchAppendix4 = 1,

    /// <summary>
    /// A regional authority's own specification — FDA, EMA, PMDA.
    /// </summary>
    /// <remarks>
    /// Nothing carries this yet. It exists because Appendix 4 defers Module 1
    /// to regional guidance, and if a regional specification is ever found to
    /// prescribe directory names, those rows must be distinguishable from the
    /// ones RegOS chose in that specification's absence.
    /// </remarks>
    RegionalSpecification = 2,

    /// <summary>
    /// RegOS's own naming convention, applied where no specification prescribes
    /// a directory name (ADR-052).
    /// </summary>
    /// <remarks>
    /// <b>This is a choice, and labelling it as one is the whole reason this
    /// enum exists.</b> It is not weaker evidence — it is not evidence at all,
    /// and a reader who mistook it for FDA's would draw a false conclusion about
    /// what the authority requires.
    /// </remarks>
    RegOsConvention = 3
}
