namespace RegOS.Submission.Application.Validation.Models;

/// <summary>
/// A single reason a submission is not ready to publish. Issues are information,
/// not failures — the validator reports them, it never throws them.
/// </summary>
public sealed record SubmissionValidationIssue(
    string Code,
    string Message,
    ValidationSeverity Severity);
