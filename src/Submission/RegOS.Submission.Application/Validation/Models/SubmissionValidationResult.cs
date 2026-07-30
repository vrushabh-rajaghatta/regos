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

    /// <summary>
    /// Everything the validator observed — blocking and advisory alike. A
    /// submission can carry issues and still be publishable; see
    /// <see cref="IsValid"/>.
    /// </summary>
    public IReadOnlyCollection<SubmissionValidationIssue> Issues => _issues.AsReadOnly();

    /// <summary>
    /// True when nothing <em>blocks</em> publishing — that is, no issue has
    /// <see cref="ValidationSeverity.Error"/> severity.
    /// </summary>
    /// <remarks>
    /// Readiness is derived from severity, not from the mere presence of an
    /// issue: a warning advises and an information issue explains, and neither
    /// should stop a submission the regulator would accept. (Counting issues
    /// made every severity blocking, which left the severity model unused.)
    /// </remarks>
    public bool IsValid =>
        !_issues.Any(issue => issue.Severity == ValidationSeverity.Error);

    /// <summary>Records a reason the submission is not ready to publish.</summary>
    public void AddIssue(
        string code,
        string message,
        ValidationSeverity severity = ValidationSeverity.Error)
    {
        _issues.Add(new SubmissionValidationIssue(code, message, severity));
    }

    /// <summary>
    /// Records a fully-formed issue — for the ones carrying more than a code and
    /// a message, such as a blueprint rule's own code or the structured list of
    /// rule types this engine cannot execute yet.
    /// </summary>
    public void AddIssue(SubmissionValidationIssue issue)
    {
        _issues.Add(issue);
    }
}
