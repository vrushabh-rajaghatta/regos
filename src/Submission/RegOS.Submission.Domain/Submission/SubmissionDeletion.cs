using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.SharedKernel.Abstractions;

namespace RegOS.Submission.Domain.Submission;

/// <summary>
/// A placement the previous sequence carried and this filing withdraws.
/// </summary>
/// <remarks>
/// <b>Why this is not a <see cref="SubmissionDocument"/>.</b> A
/// <c>SubmissionDocument</c> means <em>this dossier contains this document</em>,
/// and a deletion is precisely the absence of that. The two collections answer
/// different questions — <em>what is in this filing</em> and <em>what this
/// filing removes</em> — and merging them would need a document reference that
/// does not exist and a version that was never chosen.
/// <para>
/// It exists because the delete is <b>publication evidence like any other
/// operation</b>. Under the cumulative model (ADR-045) a withdrawal is visible
/// only as an absence, and an absence cannot be frozen — recomputing it later
/// under a changed rule would silently rewrite what a filing said. So the
/// absence is written down at publish, once, and never again.
/// </para>
/// <para>
/// Created only by <see cref="Submission.Publish"/>. There is no behaviour: a
/// filing does not change its mind about what it withdrew.
/// </para>
/// </remarks>
public sealed class SubmissionDeletion : Entity<SubmissionDeletionId>
{
    // EF materialisation only.
    private SubmissionDeletion()
    {
    }

    internal SubmissionDeletion(
        SubmissionDeletionId id,
        ProductDocumentId productDocumentId,
        TemplateSectionId templateSectionId,
        SubmissionDocumentId deletesSubmissionDocumentId)
    {
        Id = id;
        ProductDocumentId = productDocumentId;
        TemplateSectionId = templateSectionId;
        DeletesSubmissionDocumentId = deletesSubmissionDocumentId;
    }

    public ProductDocumentId ProductDocumentId { get; private set; }

    public TemplateSectionId TemplateSectionId { get; private set; }

    /// <summary>
    /// The placement in the previous sequence that this withdraws — eCTD's
    /// <c>modified-file</c> for a delete leaf.
    /// </summary>
    public SubmissionDocumentId DeletesSubmissionDocumentId { get; private set; } = default!;
}
