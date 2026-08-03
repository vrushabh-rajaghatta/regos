namespace RegOS.Submission.Domain.Submission;

public static class SubmissionErrors
{
    public const string TenantRequired =
        "Tenant is required.";

    public const string TitleRequired =
        "Submission title is required.";

    public const string ApplicationRequired =
        "Application is required.";

    // Document assembly
    public const string DocumentsLockedUnlessDraft =
        "Documents can only be changed while the submission is a draft.";

    public const string ProductDocumentAlreadyAttached =
        "This document is already attached to the submission.";

    public const string DocumentNotAttached =
        "The document is not attached to this submission.";

    public const string ProductDocumentRequired =
        "Product Document is required.";

    public const string DocumentVersionRequired =
        "Document Version is required.";

    // Placement
    public const string TemplateSectionRequired =
        "A template section is required to place a document.";

    // Lifecycle
    public const string SubmissionNotDraft =
        "Only a draft submission can be published.";

    public const string PublishedAtRequired =
        "A publication timestamp is required to publish a submission.";

    // Sequence numbering (ADR-044)
    public const string SequenceNumberNotNegative =
        "A sequence number cannot be negative.";

    public const string SequenceNumberNotContiguous =
        "A sequence must follow the previously published one — the first "
        + "sequence in an application is 0000.";

    // Content operation (ADR-045)
    public const string FirstSequenceHasNoBaseline =
        "The first sequence in an application has nothing to be compared "
        + "against.";

    // People named on the filing (ADR-048)
    public const string ContactRoleRequired =
        "A role is required to name someone on a submission.";

    public const string RolesLockedUnlessDraft =
        "Who is named on a submission can only be changed while it is a draft.";

    public const string ContactAlreadyNamedInThatRole =
        "That person is already named on this submission in that role.";

    public const string RoleNotOnSubmission =
        "That naming is not on this submission.";

    // The regulatory activity (EPIC-007a S003).
    //
    // These messages say "regulatory activity" while no type in the model does.
    // That is deliberate: the screen's word and the domain's word may differ and
    // both are binding, and an error message is a label. The model has no
    // Activity concept — an activity is derived from a chain of submissions —
    // but the person reading the message files activities for a living.
    public const string SubmissionTypeRequired =
        "A sequence that starts a new regulatory activity must say what that "
        + "activity is.";

    public const string SubmissionSubTypeRequired =
        "What this sequence does to its regulatory activity is required — it "
        + "cannot be worked out from where the sequence sits.";

    public const string OriginatingSubmissionDifferentApplication =
        "A regulatory activity cannot span two applications — the sequence that "
        + "opened it belongs to a different one.";

    public const string OriginatingSubmissionNotPublished =
        "A regulatory activity is identified by the sequence number of the "
        + "filing that opened it, and a draft has none.";

    public const string OriginatingSubmissionIsNotAnOrigin =
        "That sequence continues someone else's regulatory activity rather than "
        + "opening one. Point at the sequence that opened it.";

    public const string ClassificationLockedOncePublished =
        "What a published sequence was filed under cannot be changed — it is "
        + "what the authority received.";

    // Format (ADR-047)
    public const string FormatNotRecognised =
        "That is not a submission format RegOS recognises.";

    public const string FormatLockedOncePublished =
        "The format of a published sequence cannot be changed — it is what "
        + "the filing was made as.";

    // Studies (ADR-056)
    public const string UnplacedDocumentReportsNoStudy =
        "Which study a document reports is a fact about where it sits in the "
        + "dossier, so place it in a section first.";
}
