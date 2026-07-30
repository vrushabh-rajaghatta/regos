using RegOS.Submission.Application.Validation.Models;

using IssueSeverity = RegOS.Submission.Application.Validation.Models.ValidationSeverity;

namespace RegOS.Submission.Application.Validation.Rules;

/// <summary>
/// Checks that every document type the blueprint requires is actually attached.
/// </summary>
/// <remarks>
/// Derived from the version's <c>RequiredDocument</c> rows rather than from a
/// <c>ValidationRule</c>, so it is not an <see cref="IBlueprintRuleEvaluator"/>
/// — there is no rule type to answer for. The orchestrator runs it directly.
/// <para>
/// Coverage is answered <em>by document type</em>: "is a document of this type
/// attached?", not "is it in the right section". Placement does not exist until
/// the content plan (EPIC-003), so a type required by two sections is satisfied
/// by one attachment. Today's blueprints require each type once, so nothing is
/// masked — but the limit is real and deliberate.
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
            .Select(d => d.DocumentTypeId)
            .Distinct()
            .ToList();

        if (required.Count == 0)
            return;

        var attached = context.AttachedDocuments
            .Select(d => d.DocumentTypeId)
            .ToHashSet();

        foreach (var documentTypeId in required.Where(id => !attached.Contains(id)))
        {
            // Name the type rather than reporting a bare id: a validation issue
            // is read by a person deciding what to do next.
            result.AddIssue(
                SubmissionValidationCodes.RequiredDocumentMissing,
                $"Required document '{context.NameFor(documentTypeId)}' is missing.",
                IssueSeverity.Error);
        }
    }
}
