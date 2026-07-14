namespace RegOS.Submission.Application;

/// <summary>
/// Business-rule violation messages surfaced when creating a Submission.
/// These are cross-aggregate rules (they coordinate an Application with
/// reference data), so they live in the application layer rather than the
/// aggregate.
/// </summary>
public static class SubmissionRuleErrors
{
    public const string ApplicationDoesNotExist =
        "Application does not exist.";

    public const string SubmissionTypeDoesNotExist =
        "Submission Type does not exist.";

    public const string SubmissionTypeAuthorityMismatch =
        "Submission Type does not belong to the application's authority.";

    public const string ApplicationClosed =
        "Submission creation is not allowed for a closed application.";
}
