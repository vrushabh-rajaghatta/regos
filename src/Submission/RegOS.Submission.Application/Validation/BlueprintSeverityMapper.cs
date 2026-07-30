using BlueprintSeverity = RegOS.ReferenceData.Domain.Blueprint.ValidationSeverity;
using IssueSeverity = RegOS.Submission.Application.Validation.Models.ValidationSeverity;

namespace RegOS.Submission.Application.Validation;

/// <summary>
/// Translates how a blueprint author graded a rule into how a failure affects a
/// submission's readiness to publish.
/// </summary>
/// <remarks>
/// These are two concepts in two bounded contexts that merely happen to align
/// today, so the translation is a deliberate policy step rather than a cast.
/// <para>
/// A cast would also be a live defect: the enums do not share ordinals.
/// Blueprint <c>Error</c> is 1 and issue <c>Information</c> is 1, so
/// <c>(IssueSeverity)rule.Severity</c> would silently downgrade a blocking
/// regulatory rule to a note — and a submission that should have been stopped
/// would publish. The mapping is spelled out, and tested, for that reason.
/// </para>
/// </remarks>
public static class BlueprintSeverityMapper
{
    public static IssueSeverity ToIssueSeverity(BlueprintSeverity severity) =>
        severity switch
        {
            BlueprintSeverity.Error => IssueSeverity.Error,
            BlueprintSeverity.Warning => IssueSeverity.Warning,

            // A severity this engine does not recognise must not become the
            // weakest one by default. In a regulated system, an unknown grading
            // fails closed: block, and let a human decide.
            _ => IssueSeverity.Error,
        };
}
