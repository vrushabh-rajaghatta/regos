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

    // Format (ADR-047)
    public const string FormatNotRecognised =
        "That is not a submission format RegOS recognises.";

    public const string FormatLockedOncePublished =
        "The format of a published sequence cannot be changed — it is what "
        + "the filing was made as.";
}
