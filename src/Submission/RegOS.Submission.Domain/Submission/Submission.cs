using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Submission.Domain.Submission;

public sealed class Submission : AggregateRoot<SubmissionId>
{
    private readonly List<SubmissionDocument> _documents = [];
    private readonly List<SubmissionDeletion> _deletions = [];
    private readonly List<SubmissionStatusEntry> _history = [];
    private readonly List<SubmissionRole> _roles = [];

    // EF materialisation only.
    private Submission()
    {
    }

    private Submission(
        SubmissionId id,
        TenantId tenantId,
        RegulatoryApplicationId applicationId,
        RegulatoryTemplateVersionId? boundTemplateVersionId,
        string title,
        SubmissionFormat format,
        DateTime createdOn)
    {
        Id = id;
        TenantId = tenantId;
        ApplicationId = applicationId;
        BoundTemplateVersionId = boundTemplateVersionId;
        Title = title;
        Format = format;
        Status = SubmissionStatus.Draft;
        CreatedOn = createdOn;
    }

    // The owning tenant. Handlers derive it from the parent application
    // rather than from the ambient context, so a submission can never carry
    // a different tenant than the application it belongs to (ADR-031).
    public TenantId TenantId { get; private set; } = default!;

    public RegulatoryApplicationId ApplicationId { get; private set; }

    // The published blueprint version this submission is judged against, pinned
    // at creation so a later template version never silently changes what a
    // submission must contain. Null when no published template governs this
    // application type (device submissions today) — incomplete reference data
    // must never block creating a submission.
    public RegulatoryTemplateVersionId? BoundTemplateVersionId { get; private set; }

    public string Title { get; private set; } = default!;

    /// <summary>
    /// What this filing will be rendered as. Chosen while drafting and
    /// <b>frozen at publication</b> — you cannot change what sequence 0002 was
    /// filed as (ADR-047).
    /// </summary>
    /// <remarks>
    /// Deliberately not derived from the application: real applications moved
    /// from paper to eCTD mid-life, so format belongs to the sequence rather
    /// than to the thing the sequence belongs to.
    /// <para>
    /// Whether a later sequence may regress — eCTD at 0003, paper at 0004 — is
    /// <b>recorded, not enforced</b>. Regulators may well forbid it, but no
    /// evidence in hand says so, and inventing the rule would be the mistake
    /// this epic has avoided five times over.
    /// </para>
    /// </remarks>
    public SubmissionFormat Format { get; private set; }

    public SubmissionStatus Status { get; private set; }

    public DateTime CreatedOn { get; private set; }

    /// <summary>
    /// Every step this submission has taken, oldest first. Never empty — a
    /// submission is created as a draft, and that is a step.
    /// </summary>
    public IReadOnlyCollection<SubmissionStatusEntry> History
        => _history.AsReadOnly();

    /// <summary>
    /// When this was published, or null while a draft. **Derived, never
    /// stored** — it is the moment the <c>Published</c> entry was recorded.
    /// </summary>
    /// <remarks>
    /// It was a column until S003 added the history beside it, at which point
    /// it became a copy that could disagree with the record next to it. Same
    /// call as <c>Commitment.GivenOn</c> (ADR-042), and reached the same way:
    /// by writing the history and noticing the field was already in it.
    /// </remarks>
    public DateTime? PublishedAt
        => _history
            .LastOrDefault(x => x.Status == SubmissionStatus.Published)
            ?.RecordedOnUtc;

    /// <summary>
    /// What this submission was filed as — sequence <c>0000</c>, <c>0001</c>, …
    /// within its application. **Null means never transmitted** (ADR-044
    /// decision 4).
    /// </summary>
    /// <remarks>
    /// Assigned at publish rather than at creation, so number order <em>is</em>
    /// transmission order by construction: nothing can publish out of order, so
    /// a later sequence's diff base can never be silently rewritten. It also
    /// means an abandoned draft leaks no number, and a draft never claims a
    /// number it does not yet have — the "will publish as next sequence 0004"
    /// a user sees is derived from <c>MAX(published) + 1</c> and stored nowhere.
    /// </remarks>
    public int? SequenceNumber { get; private set; }

    // Never expose a mutable collection — the document set is only ever
    // changed through the aggregate's own behaviors.
    public IReadOnlyCollection<SubmissionDocument> Documents
        => _documents.AsReadOnly();

    /// <summary>
    /// What this filing withdrew from the previous sequence. Empty until
    /// published, and empty for most filings.
    /// </summary>
    public IReadOnlyCollection<SubmissionDeletion> Deletions
        => _deletions.AsReadOnly();

    /// <summary>
    /// Who is named on this filing, and as what (ADR-048). Empty is legitimate
    /// — a sequence that names nobody is unusual, not invalid.
    /// </summary>
    public IReadOnlyCollection<SubmissionRole> Roles => _roles.AsReadOnly();

    /// <param name="boundTemplateVersionId">
    /// The published template version that governs this submission, resolved by
    /// the application layer. Optional: when no published blueprint targets the
    /// application type, the submission is created unbound rather than rejected.
    /// </param>
    /// <param name="format">
    /// Required rather than defaulted. eCTD is the only format an FDA IND
    /// accepts today, which would make a default look harmless — but the filer
    /// chooses the format, and a default would let a caller omit a real
    /// decision and have the model answer for them.
    /// </param>
    public static Submission Create(
        TenantId tenantId,
        RegulatoryApplicationId applicationId,
        string title,
        SubmissionFormat format,
        RegulatoryTemplateVersionId? boundTemplateVersionId = null)
    {
        if (tenantId is null)
            throw new DomainException(SubmissionErrors.TenantRequired);

        if (applicationId == default)
            throw new DomainException(SubmissionErrors.ApplicationRequired);

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException(SubmissionErrors.TitleRequired);

        if (!Enum.IsDefined(format))
            throw new DomainException(SubmissionErrors.FormatNotRecognised);

        var createdOn = DateTime.UtcNow;

        var submission = new Submission(
            SubmissionId.New(),
            tenantId,
            applicationId,
            boundTemplateVersionId,
            title.Trim(),
            format,
            createdOn);

        // Becoming a draft is a step, so the history starts here rather than at
        // publication — otherwise a submission's record would begin midway
        // through its own life. The clock is read once, in Create, and used for
        // both facts rather than read twice and risk them disagreeing.
        submission.RecordStatus(
            SubmissionStatus.Draft,
            DateOnly.FromDateTime(createdOn),
            createdOn);

        return submission;
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
    /// Draft -> Published, under a sequence number. Freezes the dossier's
    /// document set and fixes what this submission was filed as. Publishing
    /// makes the submission immutable; transmission to the authority is a
    /// separate, later step.
    /// </summary>
    /// <param name="sequenceNumber">
    /// What to file this as. **Accepted, never chosen** — the aggregate supplies
    /// no number of its own, exactly as it reads no clock. A normal publish gets
    /// it from the numbering policy; an import supplies the number that was
    /// really filed (ADR-044 decision 5).
    /// </param>
    /// <param name="previousPublishedSequenceNumber">
    /// The highest sequence number already published in this submission's
    /// application, or null when this is the first.
    /// </param>
    /// <remarks>
    /// <b>The contiguity rule lives here, and its limit is worth knowing.</b>
    /// A Submission is a root; its siblings are outside its consistency
    /// boundary, so it cannot verify that the previous sequence exists — the
    /// same wall <see cref="PlaceDocument"/> documents for template sections.
    /// A caller that misreports <paramref name="previousPublishedSequenceNumber"/>
    /// gets through.
    /// <para>
    /// What makes this sound is the division of labour (ADR-044 decision 6): the
    /// unique index on (application, sequence) makes <em>duplicates</em>
    /// impossible whatever the caller does, and this rule gives <em>gaps</em> one
    /// home that a domain test can reach — rather than a convention every future
    /// handler has to remember.
    /// </para>
    /// <para>
    /// <b>Import arrives as a sibling entry point with its own name</b>, sharing
    /// a private implementation — never an <c>isImport</c> flag here. A record
    /// that already existed before RegOS is not the same business event as one
    /// we filed, and the audit history will need to tell them apart.
    /// </para>
    /// </remarks>
    /// <param name="previousPlacements">
    /// Every placement the previous published sequence carried, or empty when
    /// this is the first. The diff is computed against it and frozen — see
    /// <see cref="RecordOperations"/>.
    /// </param>
    public void Publish(
        int sequenceNumber,
        int? previousPublishedSequenceNumber,
        IReadOnlyCollection<PublishedPlacement> previousPlacements,
        DateTimeOffset publishedAt)
    {
        ArgumentNullException.ThrowIfNull(previousPlacements);

        // The application supplies the timestamp — the aggregate never reads the
        // clock, keeping Publish deterministic and testable.
        if (publishedAt == default)
            throw new DomainException(SubmissionErrors.PublishedAtRequired);

        if (sequenceNumber < 0)
            throw new DomainException(SubmissionErrors.SequenceNumberNotNegative);

        if (Status != SubmissionStatus.Draft)
            throw new BusinessRuleViolationException(
                SubmissionErrors.SubmissionNotDraft);

        // Numbering starts at 0000, so the first sequence in an application has
        // no predecessor and must be exactly zero.
        if (sequenceNumber != (previousPublishedSequenceNumber ?? -1) + 1)
            throw new BusinessRuleViolationException(
                SubmissionErrors.SequenceNumberNotContiguous);

        // A first sequence has nothing behind it, so a baseline would be a
        // caller's mistake rather than a filing's history.
        if (previousPublishedSequenceNumber is null && previousPlacements.Count > 0)
            throw new BusinessRuleViolationException(
                SubmissionErrors.FirstSequenceHasNoBaseline);

        RecordOperations(previousPlacements);

        Status = SubmissionStatus.Published;
        SequenceNumber = sequenceNumber;

        RecordStatus(
            SubmissionStatus.Published,
            DateOnly.FromDateTime(publishedAt.UtcDateTime),
            publishedAt.UtcDateTime);
    }

    /// <summary>
    /// Names a person on this filing.
    /// </summary>
    /// <remarks>
    /// <b>One assignment per (person, role)</b> — naming the same person as the
    /// same thing twice would say it twice, not doubly. Two people may share a
    /// role, and one person may hold several; neither is unusual, and the same
    /// call <c>Contact.AddRole</c> already made.
    /// <para>
    /// Whether the contact exists, is active, and belongs to this tenant is the
    /// application layer's to check — the aggregate enforces only what it can
    /// see from its own state. It deliberately does <em>not</em> check that the
    /// contact's profile lists this role (ADR-048).
    /// </para>
    /// </remarks>
    public SubmissionRole AssignRole(ContactId contactId, ContactRoleId roleId)
    {
        ArgumentNullException.ThrowIfNull(contactId);

        if (roleId == default)
            throw new DomainException(SubmissionErrors.ContactRoleRequired);

        // The freeze: who was named on a published sequence is a fact about a
        // filing already made (ADR-048), and the draft guard is the whole
        // mechanism — the same call ChangeFormat makes.
        if (Status != SubmissionStatus.Draft)
            throw new BusinessRuleViolationException(
                SubmissionErrors.RolesLockedUnlessDraft);

        if (_roles.Any(x => x.ContactId == contactId && x.RoleId == roleId))
            throw new BusinessRuleViolationException(
                SubmissionErrors.ContactAlreadyNamedInThatRole);

        var role = new SubmissionRole(SubmissionRoleId.New(), contactId, roleId);

        _roles.Add(role);

        return role;
    }

    /// <summary>
    /// Removes a naming from a draft. Not a lifecycle event — a draft that
    /// named the wrong person is corrected, not amended.
    /// </summary>
    public void RemoveRole(SubmissionRoleId submissionRoleId)
    {
        ArgumentNullException.ThrowIfNull(submissionRoleId);

        if (Status != SubmissionStatus.Draft)
            throw new BusinessRuleViolationException(
                SubmissionErrors.RolesLockedUnlessDraft);

        var role = _roles.FirstOrDefault(x => x.Id == submissionRoleId)
            ?? throw new NotFoundException(SubmissionErrors.RoleNotOnSubmission);

        _roles.Remove(role);
    }

    /// <summary>
    /// Changes what this filing will be rendered as, while it is still a draft.
    /// </summary>
    /// <remarks>
    /// The draft guard <em>is</em> the freeze required by ADR-047: a published
    /// sequence's format is a fact about a filing that has already been made,
    /// and no later decision can reach back and alter it. No separate
    /// immutability mechanism is needed, and adding one would give the same
    /// rule two places to live.
    /// </remarks>
    public void ChangeFormat(SubmissionFormat format)
    {
        if (!Enum.IsDefined(format))
            throw new DomainException(SubmissionErrors.FormatNotRecognised);

        if (Status != SubmissionStatus.Draft)
            throw new BusinessRuleViolationException(
                SubmissionErrors.FormatLockedOncePublished);

        Format = format;
    }

    /// <summary>
    /// Appends a step. Append-only: the history is what happened, so there is
    /// no amend and no remove.
    /// </summary>
    private void RecordStatus(
        SubmissionStatus status,
        DateOnly occurredOn,
        DateTime recordedOnUtc,
        string? note = null)
        => _history.Add(new SubmissionStatusEntry(
            SubmissionStatusEntryId.New(),
            status,
            occurredOn,
            recordedOnUtc,
            note));

    /// <summary>
    /// Computes what this filing did to each placement, and freezes it.
    /// </summary>
    /// <remarks>
    /// <b>The identity that survives across sequences is (document, section)</b>
    /// — <em>the same document, in the same place</em>. A
    /// <c>SubmissionDocumentId</c> belongs to one submission and cannot compare
    /// across two.
    /// <para>
    /// The rule is deliberately literal, and the two things it does <em>not</em>
    /// decide are open regulatory questions rather than oversights (EPIC-004
    /// hypotheses 4 and 5). A document that moved section reads here as a delete
    /// plus a new, because that is what the key says happened; whether a
    /// regulator would call it a replace is a question a real filing answers, at
    /// EPIC-007. Nothing produces <c>Append</c>.
    /// </para>
    /// <para>
    /// Unplaced attachments are skipped and keep a null operation: an operation
    /// is a fact about a placement.
    /// </para>
    /// </remarks>
    private void RecordOperations(
        IReadOnlyCollection<PublishedPlacement> previousPlacements)
    {
        var baseline = previousPlacements.ToDictionary(
            p => (p.ProductDocumentId, p.TemplateSectionId));

        foreach (var document in _documents)
        {
            if (document.TemplateSectionId is not { } section)
                continue;

            var key = (document.ProductDocumentId, section);

            if (!baseline.TryGetValue(key, out var previous))
            {
                document.RecordOperation(SubmissionContentOperation.New);
                continue;
            }

            document.RecordOperation(
                previous.DocumentVersionId == document.DocumentVersionId
                    ? SubmissionContentOperation.Unchanged
                    : SubmissionContentOperation.Replace,
                previous.DocumentVersionId == document.DocumentVersionId
                    ? null
                    : previous.Id);
        }

        // What the previous sequence carried and this one does not. Written down
        // rather than left as an absence: an absence cannot be frozen, and a
        // later recomputation under a changed rule would rewrite what this
        // filing said.
        var current = _documents
            .Where(d => d.TemplateSectionId is not null)
            .Select(d => (d.ProductDocumentId, Section: d.TemplateSectionId!.Value))
            .ToHashSet();

        foreach (var withdrawn in previousPlacements
            .Where(p => !current.Contains((p.ProductDocumentId, p.TemplateSectionId))))
        {
            _deletions.Add(new SubmissionDeletion(
                SubmissionDeletionId.New(),
                withdrawn.ProductDocumentId,
                withdrawn.TemplateSectionId,
                withdrawn.Id));
        }
    }
}
