using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.Submission.Domain.Submission;

/// <summary>
/// A reference to a specific version of a Product Document, included in a
/// Submission's dossier. A child entity of the <see cref="Submission"/>
/// aggregate — it has no lifecycle of its own and is only ever created and
/// removed through the aggregate.
///
/// It records the <em>selection</em> (which document, which version, in what
/// order), never the file itself. Name, status, storage, and content are read
/// through the referenced Product Document / version.
/// </summary>
public sealed class SubmissionDocument
{
    // Only the Submission aggregate may create attachments.
    internal SubmissionDocument(
        SubmissionDocumentId id,
        ProductDocumentId productDocumentId,
        DocumentVersionId documentVersionId,
        int displayOrder,
        DateTime attachedOnUtc)
    {
        Id = id;
        ProductDocumentId = productDocumentId;
        DocumentVersionId = documentVersionId;
        DisplayOrder = displayOrder;
        AttachedOnUtc = attachedOnUtc;
    }

    public SubmissionDocumentId Id { get; }

    public ProductDocumentId ProductDocumentId { get; }

    // Pinned at attach time — the dossier stays immutable even if a newer
    // version of the document is uploaded later.
    public DocumentVersionId DocumentVersionId { get; }

    public int DisplayOrder { get; }

    public DateTime AttachedOnUtc { get; }
}
