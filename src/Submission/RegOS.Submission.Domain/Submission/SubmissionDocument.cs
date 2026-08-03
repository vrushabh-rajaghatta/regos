using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.SharedKernel.Abstractions;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;

namespace RegOS.Submission.Domain.Submission;

/// <summary>
/// A reference to a specific version of a Product Document, included in a
/// Submission's dossier. A child entity of the <see cref="Submission"/>
/// aggregate — it has no lifecycle of its own and is only ever created and
/// removed through the aggregate.
///
/// It records the <em>selection</em> (which document, which version, in what
/// order) and its <em>placement</em> (where in the dossier it sits), never the
/// file itself. Name, status, storage, and content are read through the
/// referenced Product Document / version.
/// </summary>
public sealed class SubmissionDocument : Entity<SubmissionDocumentId>
{
    // EF materialisation only.
    private SubmissionDocument()
    {
    }

    // Only the Submission aggregate may create attachments.
    internal SubmissionDocument(
        SubmissionDocumentId id,
        ProductDocumentId productDocumentId,
        DocumentVersionId documentVersionId,
        int displayOrder,
        DateTime attachedAt,
        TemplateSectionId? templateSectionId = null)
    {
        Id = id;
        ProductDocumentId = productDocumentId;
        DocumentVersionId = documentVersionId;
        DisplayOrder = displayOrder;
        AttachedAt = attachedAt;
        TemplateSectionId = templateSectionId;
    }

    public ProductDocumentId ProductDocumentId { get; private set; }

    // Pinned at attach time — the dossier stays immutable even if a newer
    // version of the document is uploaded later.
    public DocumentVersionId DocumentVersionId { get; private set; }

    public int DisplayOrder { get; private set; }

    public DateTime AttachedAt { get; private set; }

    /// <summary>
    /// Where this document sits in the dossier: a section of the submission's
    /// bound template version. Null while attached but not yet placed — a
    /// legitimate intermediate state, not an error.
    /// </summary>
    /// <remarks>
    /// One section, not many. eCTD does allow the same document to appear under
    /// several sections (leaf reuse), and that capability is deliberately
    /// deferred rather than overlooked — nothing in the product exercises it
    /// yet, and the migration to a placement collection is one row per existing
    /// placement, with no inference and no data loss.
    /// <para>
    /// Placement answers <em>where a document belongs</em>. Whether it satisfies
    /// a requirement is derived from (section, document type) by the validator —
    /// the hierarchy is organisational, placeholders are a validation construct.
    /// </para>
    /// </remarks>
    public TemplateSectionId? TemplateSectionId { get; private set; }

    /// <summary>
    /// What this filing did to this placement, relative to the previously
    /// published sequence. **Null until the submission is published**, and null
    /// afterwards for a document that was never placed.
    /// </summary>
    /// <remarks>
    /// The second case is not an omission. <b>An operation is a fact about a
    /// placement, not about an attachment</b> — a document sitting nowhere in
    /// the dossier is in no section, produces no leaf, and did nothing to the
    /// previous sequence. Publishing with unplaced documents is permitted (the
    /// validator reports it as information, not an error), so the invariant
    /// worth stating is the narrower one: <em>a published submission has an
    /// operation for every placed document.</em>
    /// </remarks>
    public SubmissionContentOperation? Operation { get; private set; }

    /// <summary>
    /// The study this placement reports on, when it is a clinical one. Null
    /// when it reports a non-clinical study, or no study at all.
    /// </summary>
    /// <remarks>
    /// <b>A fact about the placement, not about the document and not about the
    /// study</b> (ADR-053, ADR-056 §4). The same
    /// <c>ProductDocument</c> can be filed in two sequences and report the same
    /// study both times; what differs is the placement. And a study does not
    /// know where it is filed — so refiling changes a row here and never
    /// touches the registry.
    /// <para>
    /// <b>Typed, and paired with <see cref="NonClinicalStudyId"/> rather than
    /// merged into one column.</b> They are two aggregates with two identity
    /// spaces (ADR-056), so a single id would be a supertype in all but name.
    /// The cost is an exclusive-or across two nullable columns, enforced by
    /// <see cref="ReportClinicalStudy"/> and its sibling, which are the only
    /// writers and each clears the other.
    /// </para>
    /// </remarks>
    public ClinicalStudyId? ClinicalStudyId { get; private set; }

    /// <summary>
    /// The study this placement reports on, when it is a non-clinical one —
    /// the Module 4 half, and the one that blocks an IND today.
    /// </summary>
    /// <remarks>See <see cref="ClinicalStudyId"/> for why there are two.</remarks>
    public NonClinicalStudyId? NonClinicalStudyId { get; private set; }

    /// <summary>
    /// True when this placement reports a study of either kind.
    /// </summary>
    public bool ReportsAStudy
        => ClinicalStudyId is not null || NonClinicalStudyId is not null;

    /// <summary>
    /// What role this document plays in that study's report — ICH's
    /// <c>file-tag</c>: <c>synopsis</c>, <c>protocol-or-amendment</c>,
    /// <c>randomisation-scheme</c> and 94 others.
    /// </summary>
    /// <remarks>
    /// <b>Stored as the published token, not translated from a domain concept.</b>
    /// The list is a wire vocabulary rather than 97 business concepts — half of
    /// it names dataset formats and regional artefacts — so ADR-055's promotion
    /// test fails for it, and it is recorded the way an application number is:
    /// verbatim, with the check at the boundary that owns the list
    /// (<c>FileTagVocabulary</c>, applied in the handler).
    /// <para>
    /// <b>The realm is not stored beside it.</b> All 97 published values are
    /// distinct across <c>ich</c>, <c>us</c> and <c>jp</c>, so <c>info-type</c>
    /// is a function of the tag — a second column could only ever disagree with
    /// the first.
    /// </para>
    /// <para>
    /// <b>Null unless this placement reports a study</b>, and that is an
    /// invariant rather than a convention: a <c>file-tag</c> exists only inside
    /// an STF, and an STF exists only for a study. Clearing the study clears
    /// this with it.
    /// </para>
    /// </remarks>
    public string? FileTag { get; private set; }

    /// <summary>
    /// The sponsor's study identifier <em>as this sequence filed it</em>, and
    /// the study's title likewise. **Null until publication**, and null
    /// afterwards for a placement that reported no study.
    /// </summary>
    /// <remarks>
    /// <b>The freeze boundary.</b> A study is mutable; a filed sequence is not:
    /// <code>
    /// Study (mutable) → Publication → frozen snapshot → STF XML
    /// </code>
    /// An STF is projected from the snapshot, never from today's registry, so
    /// regenerating sequence 0000 a year later reproduces the bytes FDA
    /// received — which is [ADR-047](ADR-047)'s instrument applied to a fact
    /// this aggregate does not own.
    /// <para>
    /// <b>It is a copy, and that is the point.</b> Everywhere else RegOS
    /// refuses to duplicate a fact because two copies can disagree; here they
    /// are <em>meant</em> to, and the disagreement is the record of a study
    /// having been renamed since it was filed.
    /// </para>
    /// </remarks>
    public string? FiledStudyIdentifier { get; private set; }

    /// <inheritdoc cref="FiledStudyIdentifier"/>
    public string? FiledStudyTitle { get; private set; }

    /// <summary>
    /// The placement in the previous sequence this supersedes — eCTD's
    /// <c>modified-file</c>. Set only alongside
    /// <see cref="SubmissionContentOperation.Replace"/>.
    /// </summary>
    /// <remarks>
    /// Derivable only at publish and meaningless afterwards without it: the
    /// pointer names one specific prior leaf, and which leaf that was depends on
    /// the derivation rule in force at the time.
    /// </remarks>
    public SubmissionDocumentId? ReplacesSubmissionDocumentId { get; private set; }

    // Only the aggregate may move a document; callers go through
    // Submission.PlaceDocument / ClearPlacement so the invariants are enforced.
    //
    // Taking a document out of the dossier takes its study with it. The study
    // is a fact about *where this document is filed*, so a document that sits
    // nowhere reports nothing — leaving the reference behind would make
    // "a fact about the placement" true in the comment and false in the row.
    internal void PlaceIn(TemplateSectionId? templateSectionId)
    {
        TemplateSectionId = templateSectionId;

        if (templateSectionId is null) ClearReportedStudy();
    }

    // The two writers of the exclusive-or. Each clears the other, so no caller
    // can produce a placement that reports two studies. Both take the file-tag,
    // because the tag is part of the same fact: what this placement contributes
    // to a study report is (which study, in what role), and a writer that set
    // half of it would leave the other half describing the study before.
    internal void ReportClinicalStudy(ClinicalStudyId studyId, string? fileTag)
    {
        ClinicalStudyId = studyId;
        NonClinicalStudyId = null;
        FileTag = fileTag;
    }

    internal void ReportNonClinicalStudy(
        NonClinicalStudyId studyId,
        string? fileTag)
    {
        NonClinicalStudyId = studyId;
        ClinicalStudyId = null;
        FileTag = fileTag;
    }

    internal void ClearReportedStudy()
    {
        ClinicalStudyId = null;
        NonClinicalStudyId = null;
        FileTag = null;
    }

    // Only Submission.Publish may set these, and only once — the snapshot of
    // what this sequence said the study was.
    internal void FreezeStudyIdentity(string identifier, string title)
    {
        FiledStudyIdentifier = identifier;
        FiledStudyTitle = title;
    }

    // Only Submission.Publish may set this, and only once.
    internal void RecordOperation(
        SubmissionContentOperation operation,
        SubmissionDocumentId? replaces = null)
    {
        Operation = operation;
        ReplacesSubmissionDocumentId = replaces;
    }
}
