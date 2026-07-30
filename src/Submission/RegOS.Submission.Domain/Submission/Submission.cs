using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Submission.Domain.Submission;

public sealed class Submission
{
    private readonly List<SubmissionDocument> _documents = [];

    private Submission(
        SubmissionId id,
        TenantId tenantId,
        RegulatoryApplicationId applicationId,
        SubmissionTypeId submissionTypeId,
        RegulatoryTemplateVersionId? boundTemplateVersionId,
        string title,
        DateTime createdOn)
    {
        Id = id;
        TenantId = tenantId;
        ApplicationId = applicationId;
        SubmissionTypeId = submissionTypeId;
        BoundTemplateVersionId = boundTemplateVersionId;
        Title = title;
        Status = SubmissionStatus.Draft;
        CreatedOn = createdOn;
    }

    public SubmissionId Id { get; }

    // The owning tenant. Handlers derive it from the parent application
    // rather than from the ambient context, so a submission can never carry
    // a different tenant than the application it belongs to (ADR-031).
    public TenantId TenantId { get; }

    public RegulatoryApplicationId ApplicationId { get; }

    public SubmissionTypeId SubmissionTypeId { get; }

    // The published blueprint version this submission is judged against, pinned
    // at creation so a later template version never silently changes what a
    // submission must contain. Null when no published template governs this
    // submission type (device submissions today) — incomplete reference data
    // must never block creating a submission.
    public RegulatoryTemplateVersionId? BoundTemplateVersionId { get; private set; }

    public string Title { get; private set; }

    public SubmissionStatus Status { get; private set; }

    public DateTime CreatedOn { get; }

    // Null while Draft; set when the submission is published. PublishedBy is
    // deferred until the project has a current-user identity to record.
    public DateTimeOffset? PublishedAt { get; private set; }

    // Never expose a mutable collection — the document set is only ever
    // changed through the aggregate's own behaviors.
    public IReadOnlyCollection<SubmissionDocument> Documents
        => _documents.AsReadOnly();

    /// <param name="boundTemplateVersionId">
    /// The published template version that governs this submission, resolved by
    /// the application layer. Optional: when no published blueprint targets the
    /// submission type, the submission is created unbound rather than rejected.
    /// </param>
    public static Submission Create(
        TenantId tenantId,
        RegulatoryApplicationId applicationId,
        SubmissionTypeId submissionTypeId,
        string title,
        RegulatoryTemplateVersionId? boundTemplateVersionId = null)
    {
        if (tenantId is null)
            throw new DomainException(SubmissionErrors.TenantRequired);

        if (applicationId == default)
            throw new DomainException(SubmissionErrors.ApplicationRequired);

        if (submissionTypeId == default)
            throw new DomainException(SubmissionErrors.SubmissionTypeRequired);

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException(SubmissionErrors.TitleRequired);

        return new Submission(
            SubmissionId.New(),
            tenantId,
            applicationId,
            submissionTypeId,
            boundTemplateVersionId,
            title.Trim(),
            DateTime.UtcNow);
    }

    /// <summary>
    /// Attaches a reference to a specific version of a Product Document,
    /// optionally placing it into a section of the dossier in the same step.
    /// The aggregate enforces only what it can see from its own state: the
    /// submission must be a draft, and a given Product Document may appear
    /// only once. Existence, product ownership, active status, version
    /// resolution, and whether the section belongs to this submission's bound
    /// template version are the application layer's responsibility.
    /// </summary>
    /// <param name="templateSectionId">
    /// Where the document sits in the dossier. Optional: "upload this into
    /// 3.2.S.2" is one user action, but attaching without placing is equally
    /// legitimate and leaves the document unplaced.
    /// </param>
    /// <returns>The newly created attachment.</returns>
    public SubmissionDocument AttachDocument(
        ProductDocumentId productDocumentId,
        DocumentVersionId documentVersionId,
        TemplateSectionId? templateSectionId = null)
    {
        if (productDocumentId == default)
            throw new DomainException(SubmissionErrors.ProductDocumentRequired);

        if (documentVersionId == default)
            throw new DomainException(SubmissionErrors.DocumentVersionRequired);

        // Rule 1 — only a draft dossier may change.
        if (Status != SubmissionStatus.Draft)
            throw new BusinessRuleViolationException(
                SubmissionErrors.DocumentsLockedUnlessDraft);

        // Rule 2 — the same document cannot be attached twice. Changing the
        // version of an existing attachment is a future capability.
        if (_documents.Any(d => d.ProductDocumentId == productDocumentId))
            throw new BusinessRuleViolationException(
                SubmissionErrors.ProductDocumentAlreadyAttached);

        // Rule 3 — the aggregate owns ordering; callers never supply it.
        var displayOrder = _documents.Count == 0
            ? 1
            : _documents.Max(d => d.DisplayOrder) + 1;

        var document = new SubmissionDocument(
            SubmissionDocumentId.New(),
            productDocumentId,
            documentVersionId,
            displayOrder,
            DateTime.UtcNow,
            templateSectionId);

        _documents.Add(document);

        return document;
    }

    /// <summary>
    /// Places an already-attached document into a section of the dossier,
    /// moving it if it was placed elsewhere.
    /// </summary>
    /// <remarks>
    /// This is a placement, never an attachment: the document must already
    /// belong to this submission. Allowing an unknown id through would turn
    /// placement into an "attach by reference" back door that bypasses every
    /// rule <see cref="AttachDocument"/> enforces — product ownership, active
    /// status, version pinning.
    /// <para>
    /// The aggregate cannot check that the section belongs to its bound
    /// template version: sections are Reference Data, and reaching across that
    /// boundary from inside the aggregate would be worse than the handler
    /// owning the rule.
    /// </para>
    /// </remarks>
    public void PlaceDocument(
        SubmissionDocumentId submissionDocumentId,
        TemplateSectionId templateSectionId)
    {
        if (templateSectionId == default)
            throw new DomainException(SubmissionErrors.TemplateSectionRequired);

        Placeable(submissionDocumentId).PlaceIn(templateSectionId);
    }

    /// <summary>
    /// Removes a document from the dossier structure without detaching it. It
    /// stays part of the submission, but sits nowhere — a state the validator
    /// reports rather than tolerates silently.
    /// </summary>
    public void ClearPlacement(SubmissionDocumentId submissionDocumentId)
        => Placeable(submissionDocumentId).PlaceIn(null);

    private SubmissionDocument Placeable(SubmissionDocumentId submissionDocumentId)
    {
        // Rule 1 — only a draft dossier may change.
        if (Status != SubmissionStatus.Draft)
            throw new BusinessRuleViolationException(
                SubmissionErrors.DocumentsLockedUnlessDraft);

        // Rule 4 — can only place something that is actually attached.
        var document = _documents.SingleOrDefault(
            d => d.Id == submissionDocumentId);

        if (document is null)
            throw new BusinessRuleViolationException(
                SubmissionErrors.DocumentNotAttached);

        return document;
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
            throw new BusinessRuleViolationException(
                SubmissionErrors.DocumentsLockedUnlessDraft);

        // Rule 4 — can only remove something that is actually attached.
        var document = _documents.SingleOrDefault(
            d => d.Id == submissionDocumentId);

        if (document is null)
            throw new BusinessRuleViolationException(
                SubmissionErrors.DocumentNotAttached);

        _documents.Remove(document);
    }

    /// <summary>
    /// Draft -> Published. Freezes the dossier's document set. Publishing makes
    /// the submission immutable; transmission to the authority is a separate,
    /// later step.
    /// </summary>
    public void Publish(DateTimeOffset publishedAt)
    {
        // The application supplies the timestamp — the aggregate never reads the
        // clock, keeping Publish deterministic and testable.
        if (publishedAt == default)
            throw new DomainException(SubmissionErrors.PublishedAtRequired);

        if (Status != SubmissionStatus.Draft)
            throw new BusinessRuleViolationException(
                SubmissionErrors.SubmissionNotDraft);

        Status = SubmissionStatus.Published;
        PublishedAt = publishedAt;
    }
}
