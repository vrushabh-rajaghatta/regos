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

    /// <remarks>
    /// The folder's twin, and it needs its own words: a section can have a
    /// folder and still have no element, which is exactly what every blueprint
    /// version before v4 looks like. <b>RegOS may never invent this value</b> —
    /// an invented element name is DTD-invalid, which is why it carries no
    /// provenance the way a folder does (ADR-052).
    /// </remarks>
    public const string NoEctdElementForSection =
        "Section {0} has no eCTD backbone element, so it cannot be written into "
        + "index.xml. The section is real; what the specification calls it has "
        + "not been read yet.";

    // ── Refusal 3 — a fact the specification needs and RegOS does not hold ───

    /// <remarks>
    /// <b>A third kind of gap, and it is neither of the other two.</b> Nobody
    /// can close this by asking whoever filed the sequence, and nobody can close
    /// it by reading a specification — the specification has been read, and it
    /// asks for a fact the domain model does not carry. Closing it means
    /// modelling something new.
    /// </remarks>
    public const string SectionNeedsAFactRegOsDoesNotHold =
        "Section {0} is written into the backbone as <{1}>, which the eCTD DTD "
        + "will not accept without naming its {2}. That node exists once per "
        + "{2}; RegOS records the section but not which one, so this leaf cannot "
        + "be written down truthfully.";

    /// <remarks>
    /// <b>The same third gap, and by far the largest instance of it.</b> FDA
    /// requires a Study Tagging File for every file in 4.2.x and 5.3.1.x–5.3.5.x
    /// (evidence E21), and an STF states which study a document belongs to and
    /// what role it plays in that study's report. RegOS holds neither fact.
    /// <para>
    /// Not closable by reading anything — the ICH specification that defines an
    /// STF has been read (E29). <b>ADR-054</b> says what one is (a projection
    /// over the placements in a sequence that belong to one study) and what has
    /// to exist before one can be written.
    /// </para>
    /// <para>
    /// <b>Refused rather than written without one</b>, and no validator could
    /// have made this decision for us: a leaf with no STF is perfectly valid
    /// XML, and FDA's review tool simply files it under *"Not Applicable (N/A)
    /// or Unassigned Folders"* (eCTD TCG §4.3). A package that quietly loses its
    /// nonclinical section on arrival is worse than one that was never built.
    /// </para>
    /// </remarks>
    public const string SectionRequiresAStudyTaggingFile =
        "Section {0} holds study reports, and FDA files nothing under <{1}> "
        + "without a Study Tagging File naming the study each document belongs "
        + "to. RegOS does not record studies yet, so there is nothing to name.";

    public const string SubmissionIsUnbound =
        "This sequence is not bound to a blueprint, so its documents sit in no "
        + "sections and there is no structure to write.";

    // ── What the authority will not accept ───────────────────────────────────

    /// <remarks>
    /// <b>The stricter of two published limits wins.</b> ICH Appendix 2 allows a
    /// 230-character path; FDA's eCTD Technical Conformance Guide §2.4 says *"the
    /// length of the entire path must not exceed 150 characters"* (evidence E22).
    /// A path can therefore be legal under ICH and rejected by the region it is
    /// filed to, and RegOS previously checked neither.
    /// <para>
    /// Refused rather than truncated: shortening a path silently renames a
    /// document inside a package a regulator reads, and the fix — a shorter
    /// document name — belongs to whoever named it.
    /// </para>
    /// </remarks>
    public const string PathTooLongForTheRegion =
        "'{0}' is {1} characters, and FDA accepts no path longer than {2}. "
        + "Shorten the document's name — every folder in the path is fixed by "
        + "the blueprint.";

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
