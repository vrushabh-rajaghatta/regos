namespace RegOS.Submission.Application.Validation;

/// <summary>
/// Stable machine-readable codes for submission validation issues. UI, API
/// consumers, localization, and analytics key off these rather than the
/// human-readable message, so the codes must not change once shipped.
/// </summary>
public static class SubmissionValidationCodes
{
    /// <summary>The submission has already been published and cannot be published again.</summary>
    public const string SubmissionAlreadyPublished = "SubmissionAlreadyPublished";

    /// <summary>The submission has no documents attached.</summary>
    public const string SubmissionHasNoDocuments = "SubmissionHasNoDocuments";

    /// <summary>An attached document version no longer exists (data integrity guard).</summary>
    public const string MissingDocumentVersion = "MissingDocumentVersion";
}
