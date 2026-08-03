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

    // Application type existence and authority-belonging used to be checked
    // here, on every submission. They moved to application creation in
    // EPIC-007a S001 — a classification is settled once, not re-proved by each
    // sequence filed under it.

    public const string ApplicationClosed =
        "Submission creation is not allowed for a closed application.";

    // The regulatory activity (EPIC-007a S003). Unlike the four invariants on
    // the aggregate, these coordinate a Submission with reference data and with
    // another Submission, so they cannot live inside a consistency boundary of
    // one.
    public const string ActivityChoiceNotExclusive =
        "A sequence either starts a new regulatory activity or continues an "
            + "existing one — say exactly one.";

    public const string SubmissionTypeDoesNotExist =
        "That kind of regulatory activity does not exist.";

    public const string SubmissionSubTypeDoesNotExist =
        "That is not something a sequence can do to a regulatory activity.";

    // The same shape as the rule S001 moved onto RegulatoryApplication.Create:
    // an FDA annual report filed under a TGA application is not a data-entry
    // slip to be caught downstream — it is not a filing. It stays here because
    // a Submission holds an ApplicationId, not an AuthorityId, and reaching for
    // the authority from inside the aggregate would cross the boundary that
    // ADR-016 draws.
    public const string SubmissionTypeNotForThisAuthority =
        "That kind of regulatory activity belongs to a different authority.";

    public const string SubmissionSubTypeNotForThisAuthority =
        "That sequence action belongs to a different authority.";

    public const string OriginatingSubmissionDoesNotExist =
        "The sequence said to have opened this regulatory activity does not "
            + "exist.";

    public const string OriginatingSubmissionNotClassified =
        "That sequence was filed before RegOS recorded regulatory activities, "
            + "so there is no activity to continue.";

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

    // Studies (cross-context: studies live in their own context, ADR-056)
    public const string StudyDoesNotExist =
        "Study does not exist.";

    public const string PlacementReportsOneStudy =
        "A document reports one study, not two. Name either a clinical or a "
            + "non-clinical study, or neither.";
}
