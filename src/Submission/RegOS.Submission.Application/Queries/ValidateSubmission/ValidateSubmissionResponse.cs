using RegOS.Submission.Application.Validation.Models;

namespace RegOS.Submission.Application.Queries.ValidateSubmission;

/// <summary>
/// The API contract for a submission's validation status. Deliberately separate from
/// the internal <see cref="SubmissionValidationResult"/> so the validator's model can
/// evolve without breaking clients.
/// </summary>
public sealed record ValidateSubmissionResponse(
    bool IsValid,
    IReadOnlyCollection<ValidationIssueResponse> Issues)
{
    /// <summary>Projects the internal validation result onto the API contract.</summary>
    public static ValidateSubmissionResponse From(SubmissionValidationResult result)
    {
        var issues = result.Issues
            .Select(issue => new ValidationIssueResponse(
                issue.Code,
                issue.Message,
                issue.Severity))
            .ToList();

        return new ValidateSubmissionResponse(result.IsValid, issues);
    }
}

/// <summary>A single validation issue as exposed over the API.</summary>
public sealed record ValidationIssueResponse(
    string Code,
    string Message,
    ValidationSeverity Severity);
