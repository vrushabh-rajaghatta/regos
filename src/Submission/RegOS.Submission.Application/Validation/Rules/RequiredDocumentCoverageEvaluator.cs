using RegOS.Submission.Application.Validation.Models;

using IssueSeverity = RegOS.Submission.Application.Validation.Models.ValidationSeverity;

namespace RegOS.Submission.Application.Validation.Rules;

/// <summary>
/// Checks that every placeholder the blueprint declares is filled — a document
/// of the required type, placed in the section that requires it.
/// </summary>
/// <remarks>
/// Derived from the version's <c>RequiredDocument</c> rows rather than from a
/// <c>ValidationRule</c>, so it is not an <see cref="IBlueprintRuleEvaluator"/>
/// — there is no rule type to answer for. The orchestrator runs it directly.
/// <para>
/// Matching is on <b>(section, document type)</b>, and exactly: a document in
/// <c>3.2.S</c> does not satisfy a placeholder in <c>3.2.S.1</c>. Regulators
/// file into the leaf, and "close enough" completeness would be worse than no
/// check at all. Parent-level satisfaction, if it is ever wanted, should be an
/// explicit blueprint rule rather than an inference made here.
/// </para>
/// <para>
/// EPIC-002 matched on document type alone and de-duplicated types, because
/// placement did not exist: a type required by two sections was satisfied by one
/// attachment. That was a stated limit of ADR-035, and it is retired here. This
/// evaluator is not gaining "support for duplicates" — it is finally validating
/// what the blueprint has always expressed.
/// </para>
/// </remarks>
public sealed class RequiredDocumentCoverageEvaluator
{
    public void Evaluate(
        BlueprintEvaluationContext context,
        SubmissionValidationResult result)
    {
        // Optional requirements are excluded: the validator answers "can this
        // proceed?", not "how could this be improved".
        var required = context.Version.RequiredDocuments
            .Where(d => d.IsMandatory)
            .ToList();

        if (required.Count == 0)
            return;

        // Unplaced documents are absent from this set by construction — a
        // document that is nowhere satisfies nothing. That they exist at all is
        // reported separately, by UnplacedDocumentEvaluator.
        var placed = context.AttachedDocuments
            .Where(d => d.TemplateSectionId is not null)
            .Select(d => (Section: d.TemplateSectionId!.Value, d.DocumentTypeId))
            .ToHashSet();

        var unmet = required
            .Where(r => !placed.Contains((r.SectionId, r.DocumentTypeId)))
            .OrderBy(r => r.Order);

        foreach (var requirement in unmet)
        {
            // Name the type and the section rather than reporting bare ids: a
            // validation issue is read by a person deciding what to do next, and
            // now that placement decides the verdict, "where" is half the answer.
            result.AddIssue(
                SubmissionValidationCodes.RequiredDocumentMissing,
                $"Required document '{context.NameFor(requirement.DocumentTypeId)}' "
                    + $"is missing from {context.SectionLabelFor(requirement.SectionId)}.",
                IssueSeverity.Error);
        }
    }
}
