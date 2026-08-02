using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.SharedKernel.Abstractions;

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
    internal void PlaceIn(TemplateSectionId? templateSectionId)
        => TemplateSectionId = templateSectionId;

    // Only Submission.Publish may set this, and only once.
    internal void RecordOperation(
        SubmissionContentOperation operation,
        SubmissionDocumentId? replaces = null)
    {
        Operation = operation;
        ReplacesSubmissionDocumentId = replaces;
    }
}
