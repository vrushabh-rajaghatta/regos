namespace RegOS.Submission.Application.Generation;

/// <summary>
/// Why a sequence folder could not be produced.
/// </summary>
/// <remarks>
/// <b>Two of these are the two refusals, and they must not converge.</b>
/// Generation can be impossible because of <i>our</i> history or because of
/// <i>the authority's</i> vocabulary, and the fixes are nothing alike: one is a
/// person deciding, the other is someone reading a specification. A single
/// "cannot generate package" would collapse a distinction the evidence register
/// exists to draw.
/// </remarks>
public static class SequenceGenerationErrors
{
    // ── The submission is not the kind of thing that has a package ───────────

    public const string SubmissionDoesNotExist =
        "Submission does not exist.";

    public const string OnlyAFiledSequenceHasAPackage =
        "A package is what was filed, so only a published sequence has one — "
        + "a draft has not been filed yet.";

    /// <remarks>
    /// ADR-047 §4 asserted the <em>derivation</em> is format-independent; this
    /// is the proof the <em>rendering</em> is not. A paper submission has a
    /// dossier and no eCTD package, and the entry point is where that is
    /// provable rather than merely intended.
    /// </remarks>
    public const string OnlyEctdSequencesAreRendered =
        "This sequence was filed as {0}, not as eCTD. There is no eCTD package "
        + "to build from it.";

    // ── Refusal 1 — a gap in our history ────────────────────────────────────

    /// <remarks>
    /// Unrecoverable by construction: sub-type is not derivable from an
    /// activity's shape (evidence E13), so nobody can work out what this
    /// sequence was without asking whoever filed it.
    /// </remarks>
    public const string SequencePredatesTheActivityModel =
        "This sequence was filed before RegOS recorded regulatory activities, "
        + "so what it did to its activity was never captured — and it cannot be "
        + "worked out from the filing. Someone who knows the filing has to say.";

    // ── Refusal 2 — a gap in what we have read ──────────────────────────────

    /// <remarks>
    /// Fixable by reading a specification, which is why it says so. The row is
    /// named because "some token is missing" sends a reader hunting through
    /// three catalogues.
    /// </remarks>
    public const string NoEctdTokenForClassification =
        "{0} '{1}' has no eCTD token, so nothing can be written for it. The "
        + "value is real; its wire form has not been read from a specification "
        + "yet.";

    public const string NoEctdFolderForSection =
        "Section {0} has no eCTD folder, so there is nowhere to put its "
        + "documents. The section is real; where it is written on disk has not "
        + "been read from a specification yet.";

    public const string SubmissionIsUnbound =
        "This sequence is not bound to a blueprint, so its documents sit in no "
        + "sections and there is no structure to write.";

    // ── What the filesystem cannot express ───────────────────────────────────

    /// <remarks>
    /// <b>Refused rather than disambiguated.</b> A silent <c>-2</c> suffix would
    /// rename a document the filer named, inside a package a regulator reads —
    /// and Appendix 4's own multi-file examples number files that were already
    /// distinct. Two documents colliding after normalisation is rare and is
    /// fixed by renaming one of them.
    /// </remarks>
    public const string TwoDocumentsWouldShareAFileName =
        "'{0}' and '{1}' are placed in the same section and would be written to "
        + "the same file name ({2}). Rename one of them.";
}
