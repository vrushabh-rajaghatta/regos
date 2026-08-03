using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.ReferenceData.Domain.SubmissionSubType;
using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;

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

    /// <summary>
    /// The published sequence that opened the regulatory activity this one
    /// continues. <b>Null means this submission opens an activity of its own</b>
    /// — and then <see cref="SubmissionTypeId"/> says what that activity is.
    /// </summary>
    /// <remarks>
    /// eCTD renders it as <c>submission-id</c>, and FDA states the rule in prose
    /// (evidence E15): <i>"If the submission … is creating a new regulatory
    /// activity, the submission-id should match the sequence number."</i> A
    /// continuing sequence repeats the opener's number instead of its own, which
    /// is what groups sequences into an activity.
    /// <para>
    /// It points at the <em>opener</em>, never at the predecessor, so no chain is
    /// ever walked — see <see cref="OriginatingSubmission.IsItselfAnOrigin"/>.
    /// </para>
    /// </remarks>
    public SubmissionId? OriginatingSubmissionId { get; private set; }

    /// <summary>
    /// What the regulatory activity <em>is</em> — original application, annual
    /// report, IND safety report. <b>Set only on the sequence that opens the
    /// activity</b>, and null on every sequence that continues one.
    /// </summary>
    /// <remarks>
    /// The exclusive-or with <see cref="OriginatingSubmissionId"/> is not a rule
    /// this aggregate checks — <see cref="SubmissionClassification"/> cannot
    /// express a violation, and a CHECK constraint covers what reaches the table
    /// without passing through code.
    /// </remarks>
    public SubmissionTypeId? SubmissionTypeId { get; private set; }

    /// <summary>
    /// What this sequence does to its activity — application, amendment, report.
    /// Required on every sequence RegOS creates.
    /// </summary>
    /// <remarks>
    /// <b>Null means the sequence predates the model</b>, and nothing else. Every
    /// submission filed before S003 was recorded without any of this, and the
    /// value cannot be recovered: sub-type is not derivable from an activity's
    /// shape (evidence E13), so there was no honest backfill and none was
    /// invented. It is the same refusal S001's migration made.
    /// <para>
    /// It is emphatically not "unknown" and not "to be worked out later". A
    /// package built from such a sequence fails by name rather than guessing —
    /// see the rendering precondition in EPIC-007a.
    /// </para>
    /// </remarks>
    public SubmissionSubTypeId? SubmissionSubTypeId { get; private set; }

    /// <summary>
    /// Whether this submission carries the classification S003 introduced. False
    /// only for sequences filed before it existed.
    /// </summary>
    public bool IsClassified => SubmissionSubTypeId is not null;

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
    /// <param name="classification">
    /// Which regulatory activity this sequence belongs to, and what it does to
    /// it. <b>Required for the same reason <paramref name="format"/> is</b>: the
    /// filer decides whether this opens something new or continues something
    /// already running, and a default would answer a regulatory question on
    /// their behalf.
    /// </param>
    public static Submission Create(
        TenantId tenantId,
        RegulatoryApplicationId applicationId,
        string title,
        SubmissionFormat format,
        SubmissionClassification classification,
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

        ArgumentNullException.ThrowIfNull(classification);

        GuardOrigin(classification, applicationId);

        var createdOn = DateTime.UtcNow;

        var submission = new Submission(
            SubmissionId.New(),
            tenantId,
            applicationId,
            boundTemplateVersionId,
            title.Trim(),
            format,
            createdOn);

        submission.Apply(classification);

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
    /// <remarks>
    /// Takes any reported study with it: a document that sits nowhere reports
    /// nothing, because reporting a study is a fact about where the document is
    /// filed (ADR-056 §4).
    /// </remarks>
    public void ClearPlacement(SubmissionDocumentId submissionDocumentId)
        => Placeable(submissionDocumentId).PlaceIn(null);

    /// <summary>
    /// Records that a placement reports a clinical study.
    /// </summary>
    /// <remarks>
    /// <b>Requires the document to be placed.</b> The study is a fact about the
    /// placement, so there has to be one — and this is what makes that sentence
    /// true of the data rather than only of the comment.
    /// <para>
    /// The aggregate cannot check the study exists: studies are another
    /// context, and reaching across that boundary from inside here would be
    /// worse than the handler owning the rule — the same division
    /// <see cref="PlaceDocument"/> draws for template sections.
    /// </para>
    /// </remarks>
    /// <param name="fileTag">
    /// What role the document plays in that study's report — ICH's
    /// <c>file-tag</c>. Optional here and refused when it names nothing: the
    /// vocabulary is 97 published tokens the handler owns, not an invariant this
    /// aggregate can state (ADR-055, and the same division
    /// <c>RecordApplicationNumber</c> draws).
    /// </param>
    public void ReportClinicalStudy(
        SubmissionDocumentId submissionDocumentId,
        ClinicalStudyId studyId,
        string? fileTag = null)
    {
        ArgumentNullException.ThrowIfNull(studyId);

        Reporting(submissionDocumentId).ReportClinicalStudy(studyId, fileTag);
    }

    /// <summary>
    /// Records that a placement reports a non-clinical study — the Module 4
    /// half, and the one FDA blocks an IND over.
    /// </summary>
    /// <remarks>See <see cref="ReportClinicalStudy"/>.</remarks>
    public void ReportNonClinicalStudy(
        SubmissionDocumentId submissionDocumentId,
        NonClinicalStudyId studyId,
        string? fileTag = null)
    {
        ArgumentNullException.ThrowIfNull(studyId);

        Reporting(submissionDocumentId).ReportNonClinicalStudy(studyId, fileTag);
    }

    /// <summary>
    /// Says that this placement reports no study after all — and so plays no
    /// role in one, which is why the file-tag goes with it. Distinct from
    /// clearing the placement, which removes the document from the dossier
    /// altogether.
    /// </summary>
    public void ClearReportedStudy(SubmissionDocumentId submissionDocumentId)
        => Placeable(submissionDocumentId).ClearReportedStudy();

    private SubmissionDocument Reporting(
        SubmissionDocumentId submissionDocumentId)
    {
        var document = Placeable(submissionDocumentId);

        if (document.TemplateSectionId is null)
            throw new BusinessRuleViolationException(
                SubmissionErrors.UnplacedDocumentReportsNoStudy);

        return document;
    }

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
    /// <param name="publishedStudies">
    /// What each reported study was called at the moment of filing. Supplied
    /// rather than read, because studies are another context (ADR-056) — and
    /// frozen here rather than looked up later, because an STF must reproduce
    /// what the authority received even after the study is renamed.
    /// </param>
    public void Publish(
        int sequenceNumber,
        int? previousPublishedSequenceNumber,
        IReadOnlyCollection<PublishedPlacement> previousPlacements,
        DateTimeOffset publishedAt,
        IReadOnlyCollection<PublishedStudy>? publishedStudies = null)
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
        FreezeStudyIdentities(publishedStudies ?? []);

        Status = SubmissionStatus.Published;
        SequenceNumber = sequenceNumber;

        RecordStatus(
            SubmissionStatus.Published,
            DateOnly.FromDateTime(publishedAt.UtcDateTime),
            publishedAt.UtcDateTime);
    }

    /// <summary>
    /// Takes the snapshot an STF is projected from.
    /// </summary>
    /// <remarks>
    /// Silent about a study it was not given: the caller resolves what the
    /// placements reference, and a placement whose study has vanished from the
    /// registry is a broken reference the generator names — not something to
    /// half-freeze here.
    /// </remarks>
    private void FreezeStudyIdentities(
        IReadOnlyCollection<PublishedStudy> studies)
    {
        foreach (var document in _documents)
        {
            var studyId = document.ClinicalStudyId?.Value
                ?? document.NonClinicalStudyId?.Value;

            if (studyId is not { } id) continue;

            var study = studies.FirstOrDefault(s => s.StudyId == id);

            if (study is null) continue;

            document.FreezeStudyIdentity(study.Identifier, study.Title);
        }
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
    /// Changes which regulatory activity this sequence belongs to, or what it
    /// does to it, while it is still a draft.
    /// </summary>
    /// <remarks>
    /// The draft guard <em>is</em> the freeze, exactly as it is for
    /// <see cref="ChangeFormat"/> (ADR-047): what a published sequence was filed
    /// under is a fact the authority also holds, and no later decision may reach
    /// back and alter it.
    /// <para>
    /// A sequence may legitimately move between activities while drafting — the
    /// filer realises the change belongs to the annual report rather than a new
    /// amendment — so this is a correction, not a lifecycle event.
    /// </para>
    /// </remarks>
    public void Reclassify(SubmissionClassification classification)
    {
        ArgumentNullException.ThrowIfNull(classification);

        if (Status != SubmissionStatus.Draft)
            throw new BusinessRuleViolationException(
                SubmissionErrors.ClassificationLockedOncePublished);

        GuardOrigin(classification, ApplicationId);

        Apply(classification);
    }

    /// <summary>
    /// The three rules about the origin that the aggregate cannot see for
    /// itself, checked against facts the application layer supplied.
    /// </summary>
    /// <remarks>
    /// The fourth rule — that a submission cannot both open an activity and
    /// continue one — is absent because
    /// <see cref="SubmissionClassification"/> makes it unconstructible.
    /// </remarks>
    private static void GuardOrigin(
        SubmissionClassification classification,
        RegulatoryApplicationId applicationId)
    {
        if (classification.Origin is not { } origin)
            return;

        // An activity lives inside one application. Two applications sharing an
        // activity would render a submission-id that means nothing in either.
        if (origin.ApplicationId != applicationId)
            throw new BusinessRuleViolationException(
                SubmissionErrors.OriginatingSubmissionDifferentApplication);

        // eCTD identifies the activity by the opener's sequence number, and a
        // draft has none (ADR-044 assigns at publish). There would be nothing
        // to write.
        if (origin.SequenceNumber is null)
            throw new BusinessRuleViolationException(
                SubmissionErrors.OriginatingSubmissionNotPublished);

        // Point at the opener, never at the predecessor — otherwise rendering
        // would have to resolve a chain, and a broken link would surface as a
        // malformed package rather than as a refusal here.
        if (!origin.IsItselfAnOrigin)
            throw new BusinessRuleViolationException(
                SubmissionErrors.OriginatingSubmissionIsNotAnOrigin);
    }

    private void Apply(SubmissionClassification classification)
    {
        OriginatingSubmissionId = classification.Origin?.Id;
        SubmissionTypeId = classification.SubmissionTypeId;
        SubmissionSubTypeId = classification.SubmissionSubTypeId;
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
