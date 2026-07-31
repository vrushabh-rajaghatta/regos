using RegOS.Submission.Application.Validation.Models;

using IssueSeverity = RegOS.Submission.Application.Validation.Models.ValidationSeverity;

namespace RegOS.Submission.Application.Validation.Rules;

/// <summary>
/// Reports documents that are attached to the submission but sit nowhere in the
/// dossier structure.
/// </summary>
/// <remarks>
/// The complement of <see cref="RequiredDocumentCoverageEvaluator"/>, and
/// deliberately a second evaluator rather than a branch inside it. Coverage asks
/// <em>is every placeholder satisfied?</em> and drives completeness; this asks
/// <em>is every document somewhere?</em> and drives organisation. Merging them
/// would make coverage accumulate responsibilities unrelated to completeness.
/// <para>
/// Informational, never blocking: an unplaced document is untidy, not invalid.
/// It is also not silence — an attachment that satisfies nothing and is never
/// mentioned is exactly how a dossier gets published with a document its author
/// believed was counted.
/// </para>
/// <para>
/// The message carries a count, not names. The content plan is the authoritative
/// structured view of <em>which</em> documents are unplaced, and teaching the
/// validation response to reproduce dossier structure would create a second
/// representation to keep in sync — as well as a message that grows without
/// bound as a submission does.
/// </para>
/// </remarks>
public sealed class UnplacedDocumentEvaluator
{
    public void Evaluate(
        BlueprintEvaluationContext context,
        SubmissionValidationResult result)
    {
        var unplaced = context.AttachedDocuments
            .Count(d => d.TemplateSectionId is null);

        if (unplaced == 0)
            return;

        result.AddIssue(
            SubmissionValidationCodes.DocumentsNotPlaced,
            unplaced == 1
                ? "1 attached document has not been placed into the dossier "
                    + "structure, so it satisfies no requirement."
                : $"{unplaced} attached documents have not been placed into the "
                    + "dossier structure, so they satisfy no requirement.",
            IssueSeverity.Information);
    }
}
