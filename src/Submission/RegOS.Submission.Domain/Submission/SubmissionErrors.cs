namespace RegOS.Submission.Domain.Submission;

public static class SubmissionErrors
{
    public const string TenantRequired =
        "Tenant is required.";

    public const string TitleRequired =
        "Submission title is required.";

    public const string ApplicationRequired =
        "Application is required.";

    public const string SubmissionTypeRequired =
        "Submission Type is required.";

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

    // Lifecycle
    public const string SubmissionNotDraft =
        "Only a draft submission can be published.";

    public const string PublishedAtRequired =
        "A publication timestamp is required to publish a submission.";
}
