namespace RegOS.Submission.Application.Validation.Models;

/// <summary>
/// The outcome of validating a submission's readiness to publish. The validator
/// builds this up by adding issues; consumers only read it. <see cref="IsValid"/>
/// is always derived from the current issues, so the result can never become
/// internally inconsistent.
/// </summary>
public sealed class SubmissionValidationResult
{
    private readonly List<SubmissionValidationIssue> _issues = [];

    /// <summary>The issues found, if any. Empty when the submission is ready to publish.</summary>
    public IReadOnlyCollection<SubmissionValidationIssue> Issues => _issues.AsReadOnly();

    /// <summary>True when no issues were found.</summary>
    public bool IsValid => _issues.Count == 0;

    /// <summary>Records a reason the submission is not ready to publish.</summary>
    public void AddIssue(
        string code,
        string message,
        ValidationSeverity severity = ValidationSeverity.Error)
    {
        _issues.Add(new SubmissionValidationIssue(code, message, severity));
    }
}
