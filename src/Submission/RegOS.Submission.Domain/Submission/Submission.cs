using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Submission.Domain.Submission;

public sealed class Submission
{
    private readonly List<SubmissionDocument> _documents = [];

    private Submission(
        SubmissionId id,
        RegulatoryApplicationId applicationId,
        SubmissionTypeId submissionTypeId,
        string name,
        DateTime createdOn)
    {
        Id = id;
        ApplicationId = applicationId;
        SubmissionTypeId = submissionTypeId;
        Name = name;
        Status = SubmissionStatus.Draft;
        CreatedOn = createdOn;
    }

    public SubmissionId Id { get; }

    public RegulatoryApplicationId ApplicationId { get; }

    public SubmissionTypeId SubmissionTypeId { get; }

    public string Name { get; private set; }

    public SubmissionStatus Status { get; private set; }

    public DateTime CreatedOn { get; }

    // Never expose a mutable collection — the document set is only ever
    // changed through the aggregate's own behaviors.
    public IReadOnlyCollection<SubmissionDocument> Documents
        => _documents.AsReadOnly();

    public static Submission Create(
        RegulatoryApplicationId applicationId,
        SubmissionTypeId submissionTypeId,
        string name)
    {
        if (applicationId == default)
            throw new ArgumentException(
                SubmissionErrors.ApplicationRequired,
                nameof(applicationId));

        if (submissionTypeId == default)
            throw new ArgumentException(
                SubmissionErrors.SubmissionTypeRequired,
                nameof(submissionTypeId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                SubmissionErrors.NameRequired,
                nameof(name));

        return new Submission(
            SubmissionId.New(),
            applicationId,
            submissionTypeId,
            name.Trim(),
            DateTime.UtcNow);
    }

    /// <summary>
    /// Attaches a reference to a specific version of a Product Document.
    /// The aggregate enforces only what it can see from its own state: the
    /// submission must be a draft, and a given Product Document may appear
    /// only once. Existence, product ownership, active status, and version
    /// resolution are the application layer's responsibility.
    /// </summary>
    public void AttachDocument(
        ProductDocumentId productDocumentId,
        DocumentVersionId documentVersionId)
    {
        if (productDocumentId == default)
            throw new ArgumentException(
                SubmissionErrors.ProductDocumentRequired,
                nameof(productDocumentId));

        if (documentVersionId == default)
            throw new ArgumentException(
                SubmissionErrors.DocumentVersionRequired,
                nameof(documentVersionId));

        // Rule 1 — only a draft dossier may change.
        if (Status != SubmissionStatus.Draft)
            throw new InvalidOperationException(
                SubmissionErrors.DocumentsLockedUnlessDraft);

        // Rule 2 — the same document cannot be attached twice. Changing the
        // version of an existing attachment is a future capability.
        if (_documents.Any(d => d.ProductDocumentId == productDocumentId))
            throw new InvalidOperationException(
                SubmissionErrors.ProductDocumentAlreadyAttached);

        // Rule 3 — the aggregate owns ordering; callers never supply it.
        var displayOrder = _documents.Count == 0
            ? 1
            : _documents.Max(d => d.DisplayOrder) + 1;

        _documents.Add(new SubmissionDocument(
            SubmissionDocumentId.New(),
            productDocumentId,
            documentVersionId,
            displayOrder,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Removes an attachment from the dossier. Remaining attachments keep
    /// their display order — gaps (e.g. 1, 3) are acceptable until reordering
    /// is implemented.
    /// </summary>
    public void RemoveDocument(SubmissionDocumentId submissionDocumentId)
    {
        // Rule 1 — only a draft dossier may change.
        if (Status != SubmissionStatus.Draft)
            throw new InvalidOperationException(
                SubmissionErrors.DocumentsLockedUnlessDraft);

        // Rule 4 — can only remove something that is actually attached.
        var document = _documents.SingleOrDefault(
            d => d.Id == submissionDocumentId);

        if (document is null)
            throw new InvalidOperationException(
                SubmissionErrors.DocumentNotAttached);

        _documents.Remove(document);
    }

    /// <summary>Draft -> Submitted. Freezes the dossier's document set.</summary>
    public void Submit()
    {
        if (Status != SubmissionStatus.Draft)
            throw new InvalidOperationException(
                SubmissionErrors.SubmissionNotDraft);

        Status = SubmissionStatus.Submitted;
    }
}
