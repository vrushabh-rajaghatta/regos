namespace RegOS.Submission.Domain.Submission;

/// <summary>
/// What a filing will be rendered as when it leaves RegOS.
/// </summary>
/// <remarks>
/// <para>
/// <b>Format is a rendering concern, and the delta is not</b> (ADR-047). What a
/// sequence changed relative to the one before it is a fact about the regulatory
/// dossier — <see cref="Submission.Publish"/> derives it for every submission,
/// whatever the format. Whether that delta then leaves as an eCTD XML backbone,
/// a NeeS folder, or a paper cover letter listing the changes is downstream of
/// the model, not part of it.
/// </para>
/// <para>
/// This matters more than it looks. ADR-045 records the cumulative dossier as
/// the <em>product thesis</em>: the user owns regulatory state and RegOS derives
/// the transmitted increment. If operation derivation ran only for
/// <see cref="Ectd"/>, that thesis would quietly become an eCTD implementation
/// detail. <c>SubmissionContentOperationTests</c> asserts it does not.
/// </para>
/// <para>
/// <b>What is deliberately not here:</b> DTD versions (ICH, Regional, STF) and
/// gateway format. Both are real eCTD identity in DIA's model, and neither is a
/// fact until a package has actually been built — see ADR-047. They arrive with
/// the publishing engine in EPIC-007, which is the first thing able to state
/// them truthfully.
/// </para>
/// </remarks>
public enum SubmissionFormat
{
    /// <summary>
    /// The electronic Common Technical Document. Mandatory for FDA INDs today,
    /// which is why it is the only value the first vertical produces — but not
    /// the only one the model admits, because a real application's early
    /// sequences may predate it.
    /// </summary>
    Ectd = 1,

    /// <summary>
    /// Non-eCTD electronic Submission — electronic, but with no XML backbone
    /// and so no leaf-level lifecycle. The delta is still derived; it simply
    /// has nowhere structural to be written.
    /// </summary>
    Nees = 2,

    /// <summary>
    /// Paper. Still legal in places, and still the honest answer for sequences
    /// filed before an application moved electronic.
    /// </summary>
    Paper = 3
}
