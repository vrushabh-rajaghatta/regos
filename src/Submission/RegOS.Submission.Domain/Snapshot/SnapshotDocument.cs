using RegOS.ProductDocument.Domain.IDs;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Submission.Domain.Snapshot;

/// <summary>
/// One document in a published dossier: a frozen reference to a specific,
/// immutable Product Document version, in a fixed position. A child entity of
/// the <see cref="SubmissionSnapshot"/> aggregate — it is only ever created by
/// the snapshot's factory and never changes afterwards.
/// </summary>
/// <remarks>
/// It records only the <see cref="DocumentVersionId"/>. The owning Product
/// Document is derivable from the version, so it is not duplicated here — the
/// legal record is "version 7 of this document," and the version is the
/// immutable identity.
/// </remarks>
public sealed class SnapshotDocument
{
    // Only the SubmissionSnapshot aggregate may create snapshot documents. The
    // entity owns its own invariants: it must reference a version, and its position
    // in the dossier must be a real (positive) order.
    internal SnapshotDocument(
        SnapshotDocumentId id,
        DocumentVersionId documentVersionId,
        int displayOrder)
    {
        if (documentVersionId == default)
            throw new DomainException("A snapshot document must reference a document version.");

        if (displayOrder <= 0)
            throw new DomainException("Display order must be greater than zero.");

        Id = id;
        DocumentVersionId = documentVersionId;
        DisplayOrder = displayOrder;
    }

    public SnapshotDocumentId Id { get; }

    public DocumentVersionId DocumentVersionId { get; }

    public int DisplayOrder { get; }
}
