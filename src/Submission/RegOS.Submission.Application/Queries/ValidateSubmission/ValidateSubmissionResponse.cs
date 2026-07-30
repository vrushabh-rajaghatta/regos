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
    /// <remarks>
    /// Issues come back in a deterministic order — most severe first, then by
    /// code, rule code and message. Ordering belongs to the contract rather than
    /// to one client: users build spatial memory of a validation screen they
    /// revisit after every change, and every consumer (UI, exports, telemetry)
    /// should see the same sequence.
    /// </remarks>
    public static ValidateSubmissionResponse From(SubmissionValidationResult result)
    {
        var issues = result.Issues
            .OrderByDescending(issue => issue.Severity)
            .ThenBy(issue => issue.Code, StringComparer.Ordinal)
            .ThenBy(issue => issue.RuleCode ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(issue => issue.Message, StringComparer.Ordinal)
            .Select(issue => new ValidationIssueResponse(
                issue.Code,
                issue.Message,
                issue.Severity,
                issue.RuleCode,
                issue.UnevaluatedRuleTypes))
            .ToList();

        return new ValidateSubmissionResponse(result.IsValid, issues);
    }
}

/// <summary>A single validation issue as exposed over the API.</summary>
/// <param name="RuleCode">
/// The blueprint rule behind the issue (e.g. <c>FDA-IND-PDF</c>), when one
/// produced it. Null for the validator's own rules.
/// </param>
/// <param name="UnevaluatedRuleTypes">
/// Structured detail for the "not evaluated" disclosure, so clients never parse
/// the message text. Null on every other issue.
/// </param>
public sealed record ValidationIssueResponse(
    string Code,
    string Message,
    ValidationSeverity Severity,
    string? RuleCode = null,
    IReadOnlyList<string>? UnevaluatedRuleTypes = null);
