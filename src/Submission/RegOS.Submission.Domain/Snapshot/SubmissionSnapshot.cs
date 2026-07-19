using RegOS.ProductDocument.Domain.IDs;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Domain.Snapshot;

/// <summary>
/// The immutable, permanent record of exactly what a Submission published: which
/// document versions, in what order. A sibling aggregate of
/// <see cref="Submission.Submission"/> within the Submission bounded context —
/// it has no independent business existence and is always created from a
/// published submission.
/// </summary>
/// <remarks>
/// The snapshot deliberately stores almost nothing beyond its document set. Title,
/// submission type, application, and publish metadata are not duplicated: the
/// Submission is immutable once published, so it remains the single source of
/// truth for that information. The snapshot exists to freeze the one thing that
/// otherwise could drift — the exact set and order of document versions.
///
/// It is created once, through <see cref="Create"/>, and never changes. There are
/// no behaviors beyond creation: no add, remove, rename, or update.
/// </remarks>
public sealed class SubmissionSnapshot
{
    private readonly List<SnapshotDocument> _documents = [];

    private SubmissionSnapshot(
        SubmissionSnapshotId id,
        SubmissionId submissionId)
    {
        Id = id;
        SubmissionId = submissionId;
    }

    public SubmissionSnapshotId Id { get; }

    // A snapshot always belongs to exactly one submission (Invariant 1).
    public SubmissionId SubmissionId { get; }

    // The frozen document manifest, in display order. Never exposed as mutable —
    // the snapshot cannot be modified after creation (Invariant 4).
    public IReadOnlyCollection<SnapshotDocument> Documents
        => _documents.AsReadOnly();

    /// <summary>
    /// Creates the immutable snapshot from data the application has already extracted
    /// from the published submission. Takes only what the snapshot needs — never the
    /// Submission aggregate itself — keeping snapshots independent of other aggregates,
    /// exactly as Submission does not accept a RegulatoryApplication. The snapshot
    /// generates its own ids and creates every child entity.
    /// </summary>
    /// <param name="submissionId">The submission this snapshot preserves.</param>
    /// <param name="documents">
    /// The pinned document versions with the display order each should keep, copied
    /// exactly as published (Invariant 2). Display order must be unique across the set
    /// (Invariant 3) — the aggregate enforces that cross-document rule here; each
    /// document's own invariants are enforced by <see cref="SnapshotDocument"/>.
    /// </param>
    public static SubmissionSnapshot Create(
        SubmissionId submissionId,
        IEnumerable<(DocumentVersionId VersionId, int DisplayOrder)> documents)
    {
        if (submissionId == default)
            throw new ArgumentException(
                "A snapshot must belong to a submission.",
                nameof(submissionId));

        ArgumentNullException.ThrowIfNull(documents);

        var snapshot = new SubmissionSnapshot(
            SubmissionSnapshotId.New(),
            submissionId);

        var seenDisplayOrders = new HashSet<int>();
        foreach (var (versionId, displayOrder) in documents)
        {
            // Cross-document invariant (Invariant 3) — belongs to the aggregate.
            if (!seenDisplayOrders.Add(displayOrder))
                throw new ArgumentException(
                    "Snapshot document display order must be unique.",
                    nameof(documents));

            snapshot._documents.Add(new SnapshotDocument(
                SnapshotDocumentId.New(),
                versionId,
                displayOrder));
        }

        return snapshot;
    }
}
