using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.Submission.Application.Validation.Models;

namespace RegOS.Submission.Application.Validation.Rules;

/// <summary>
/// Executes <see cref="ValidationRuleType.SectionNotEmpty"/> rules: the section
/// a rule targets must contain at least one document.
/// </summary>
/// <remarks>
/// Deferred through the whole of EPIC-002 and disclosed as unevaluated, because
/// it asks where documents sit and nothing recorded that. Placement (STORY-001)
/// is what made it answerable.
/// <para>
/// Scope is the section and everything beneath it — see
/// <see cref="BlueprintEvaluationContext.DocumentsIn"/> for why that differs
/// from how placeholder coverage matches.
/// </para>
/// </remarks>
public sealed class SectionNotEmptyEvaluator : IBlueprintRuleEvaluator
{
    public bool CanEvaluate(ValidationRule rule)
    {
        if (rule.RuleType != ValidationRuleType.SectionNotEmpty)
            return false;

        // A rule of this type with no section names nothing to check. Disclosed
        // as unevaluated rather than vacuously passed, or silently widened to
        // "the dossier must not be empty" — which is a different rule, and not
        // one the author wrote.
        return rule.SectionId is not null;
    }

    public void Evaluate(
        ValidationRule rule,
        BlueprintEvaluationContext context,
        SubmissionValidationResult result)
    {
        var sectionId = rule.SectionId!.Value;

        if (context.DocumentsIn(sectionId).Count > 0)
            return;

        result.AddIssue(new SubmissionValidationIssue(
            SubmissionValidationCodes.BlueprintRuleViolation,
            $"{rule.Message} No documents are placed in "
                + $"{context.SectionLabelFor(sectionId)}.",
            BlueprintSeverityMapper.ToIssueSeverity(rule.Severity),
            RuleCode: rule.Code));
    }
}
