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

    /// <summary>
    /// A document type the bound blueprint requires is not attached. Blocking.
    /// </summary>
    public const string RequiredDocumentMissing = "RequiredDocumentMissing";

    /// <summary>
    /// Documents are attached but not placed into any section of the blueprint,
    /// so they satisfy nothing. Informational — untidy, not invalid. Which
    /// documents they are is the content plan's answer, not this issue's.
    /// </summary>
    public const string DocumentsNotPlaced = "DocumentsNotPlaced";

    /// <summary>
    /// No published blueprint governs this submission, so its completeness was
    /// not checked. Informational — an unbound submission is a legitimate
    /// state, not a failure.
    /// </summary>
    public const string SubmissionNotBoundToBlueprint =
        "SubmissionNotBoundToBlueprint";

    /// <summary>
    /// A rule carried by the bound blueprint was violated. The rule's own code
    /// (e.g. <c>FDA-IND-PDF</c>) travels on the issue's <c>RuleCode</c>, keeping
    /// this set of codes closed while preserving regulatory traceability.
    /// </summary>
    public const string BlueprintRuleViolation = "BlueprintRuleViolation";

    /// <summary>
    /// The blueprint carries rule types this version of the engine cannot
    /// execute yet. Informational, and a statement about the validator's
    /// capability — not a claim that those rules passed or failed.
    /// </summary>
    public const string BlueprintRulesNotEvaluated = "BlueprintRulesNotEvaluated";
}
