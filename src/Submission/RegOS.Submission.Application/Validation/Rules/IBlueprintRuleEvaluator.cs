using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.Submission.Application.Validation.Models;

namespace RegOS.Submission.Application.Validation.Rules;

/// <summary>
/// Executes one family of blueprint validation rules.
/// </summary>
/// <remarks>
/// Adding a rule type (Regex, MaxFileSize, Cardinality…) is one implementation
/// plus one registration — never a new branch in an existing evaluator, and
/// never a switch in the orchestrator.
/// <para>
/// Note this covers the blueprint's explicit <see cref="ValidationRule"/> rows.
/// Checks derived from the blueprint's structure — required-document coverage
/// today, placement completeness later — have no rule type and so are not
/// modelled here; the orchestrator runs them as their own step.
/// </para>
/// </remarks>
public interface IBlueprintRuleEvaluator
{
    /// <summary>
    /// Whether this evaluator can execute the given rule — judged on the whole
    /// rule, not only its type, because a rule can be well-formed and still be
    /// out of reach (a section-scoped rule needs document placement, which does
    /// not exist yet). Anything answered <c>false</c> is disclosed as
    /// unevaluated rather than passed over in silence.
    /// </summary>
    bool CanEvaluate(ValidationRule rule);

    void Evaluate(
        ValidationRule rule,
        BlueprintEvaluationContext context,
        SubmissionValidationResult result);
}
