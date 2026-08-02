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

    // Document assembly (attach / remove)
    public const string SubmissionDoesNotExist =
        "Submission does not exist.";

    public const string ProductDocumentDoesNotExist =
        "Product Document does not exist.";

    public const string ProductDocumentNotActive =
        "Only an active Product Document can be attached.";

    public const string ProductDocumentNotInSameProduct =
        "The Product Document belongs to a different product.";

    public const string ProductDocumentHasNoCurrentVersion =
        "The Product Document has no current version to attach.";

    // People named on the filing (ADR-048)
    public const string ContactDoesNotExist =
        "Contact does not exist.";

    public const string ContactNotActive =
        "Only an active contact can be named on a submission.";

    public const string ContactRoleDoesNotExist =
        "Contact Role does not exist.";

    // Placement (cross-context: the section lives in Reference Data)
    public const string SubmissionHasNoBlueprintToPlaceInto =
        "This submission is not bound to a blueprint, so its documents cannot "
            + "be placed into sections.";

    public const string TemplateSectionNotInBoundBlueprint =
        "The section does not belong to the blueprint this submission is "
            + "bound to.";
}
