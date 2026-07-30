namespace RegOS.Submission.Application.Validation.Models;

/// <summary>
/// A single reason a submission is not ready to publish. Issues are information,
/// not failures — the validator reports them, it never throws them.
/// </summary>
/// <param name="Code">
/// A stable identifier from the closed set in
/// <c>SubmissionValidationCodes</c>. Consumers switch on this, so it never
/// carries data-driven values.
/// </param>
/// <param name="RuleCode">
/// The blueprint rule this issue came from (e.g. <c>FDA-IND-PDF</c>), when one
/// did. Regulatory traceability lives here rather than in <paramref name="Code"/>,
/// so the closed set of codes stays closed.
/// </param>
/// <param name="UnevaluatedRuleTypes">
/// For the disclosure issue only: the blueprint rule types this version of the
/// engine cannot execute yet. Structured, so a UI or a test never has to parse
/// the message text.
/// </param>
public sealed record SubmissionValidationIssue(
    string Code,
    string Message,
    ValidationSeverity Severity,
    string? RuleCode = null,
    IReadOnlyList<string>? UnevaluatedRuleTypes = null);
