namespace RegOS.Submission.Domain.Submission;

/// <summary>
/// What a filing did to one placement, relative to the previously published
/// sequence in the same application (ADR-045).
/// </summary>
/// <remarks>
/// Computed at publish and frozen. It is not recomputed later, because the
/// <em>rule</em> that produces it is not immutable the way its inputs are: a
/// filing must keep saying what it said, even after we change our mind about
/// how the operation is derived.
/// </remarks>
public enum SubmissionContentOperation
{
    /// <summary>
    /// Carried forward from the previous sequence, byte for byte — same document,
    /// same section, same version.
    /// </summary>
    /// <remarks>
    /// eCTD has no such operation: an unchanged leaf simply does not appear in
    /// the backbone. RegOS needs the value anyway, because a submission is the
    /// <b>cumulative</b> dossier — the document is genuinely in this filing, and
    /// "nothing happened to it" has to be distinguishable from "not filed yet",
    /// which is what a null operation means. EPIC-007 emits nothing for these.
    /// </remarks>
    Unchanged = 1,

    /// <summary>Not in the previous sequence at this placement.</summary>
    New = 2,

    /// <summary>
    /// Same document in the same section, at a different version. Carries a
    /// pointer to the placement it supersedes — eCTD's <c>modified-file</c>.
    /// </summary>
    Replace = 3,

    /// <summary>
    /// Added to, rather than superseded. **Defined and never produced.**
    /// </summary>
    /// <remarks>
    /// eCTD defines it; whether FDA practice exercises it is a regulatory
    /// question this project has no evidence for, and carrying an unexercised
    /// derivation would be guessing in code (EPIC-004 hypothesis 5, resolved at
    /// EPIC-007). The value exists so a later story adds a rule rather than a
    /// migration.
    /// </remarks>
    Append = 4,

    /// <summary>
    /// A placement the previous sequence carried and this one does not.
    /// </summary>
    /// <remarks>
    /// The only operation with no <c>SubmissionDocument</c> behind it — the
    /// document is not in this dossier, which is the entire point. It is
    /// recorded as a <see cref="SubmissionDeletion"/>.
    /// </remarks>
    Delete = 5
}
