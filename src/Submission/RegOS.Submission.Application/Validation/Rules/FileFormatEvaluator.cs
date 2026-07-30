using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.Submission.Application.Validation.Models;

namespace RegOS.Submission.Application.Validation.Rules;

/// <summary>
/// Executes <see cref="ValidationRuleType.FileFormat"/> rules: every attached
/// document must be one of the formats the blueprint accepts (parameters, e.g.
/// <c>"pdf"</c> or <c>"pdf,docx"</c>).
/// </summary>
public sealed class FileFormatEvaluator : IBlueprintRuleEvaluator
{
    public bool CanEvaluate(ValidationRule rule)
    {
        if (rule.RuleType != ValidationRuleType.FileFormat)
            return false;

        // A rule with no accepted formats states nothing to check; treat it as
        // unevaluated (and therefore disclosed) rather than vacuously passing.
        if (AcceptedFormats(rule).Count == 0)
            return false;

        // Section-scoped rules ask "which documents belong to this section?" —
        // a question document placement answers, and placement does not exist
        // yet. Version-wide rules apply to the whole dossier, so they can run.
        return rule.SectionId is null;
    }

    public void Evaluate(
        ValidationRule rule,
        BlueprintEvaluationContext context,
        SubmissionValidationResult result)
    {
        var accepted = AcceptedFormats(rule);
        var severity = BlueprintSeverityMapper.ToIssueSeverity(rule.Severity);

        foreach (var document in context.AttachedDocuments)
        {
            var format = FormatOf(document);

            // Inability to establish compliance is not compliance: a document
            // whose format cannot be determined is reported, not waved through.
            if (format is null)
            {
                result.AddIssue(new SubmissionValidationIssue(
                    SubmissionValidationCodes.BlueprintRuleViolation,
                    $"{rule.Message} The format of '{document.OriginalFileName}' "
                        + "could not be determined.",
                    severity,
                    RuleCode: rule.Code));

                continue;
            }

            if (accepted.Contains(format))
                continue;

            result.AddIssue(new SubmissionValidationIssue(
                SubmissionValidationCodes.BlueprintRuleViolation,
                $"{rule.Message} '{document.OriginalFileName}' is a "
                    + $"'{format}' file; accepted: {string.Join(", ", accepted)}.",
                severity,
                RuleCode: rule.Code));
        }
    }

    /// <summary>
    /// The formats a rule accepts, normalised — comma-separated, case- and
    /// dot-insensitive, so <c>"PDF, .docx"</c> and <c>"pdf,docx"</c> agree.
    /// </summary>
    private static IReadOnlyCollection<string> AcceptedFormats(ValidationRule rule)
    {
        if (string.IsNullOrWhiteSpace(rule.Parameters))
            return [];

        return rule.Parameters
            .Split(',', StringSplitOptions.RemoveEmptyEntries
                | StringSplitOptions.TrimEntries)
            .Select(Normalize)
            .Where(f => f.Length > 0)
            .ToHashSet();
    }

    /// <summary>
    /// The document's format: the filename extension first, the content type
    /// only as a fallback.
    /// </summary>
    /// <remarks>
    /// Filenames are what a user sees and what a reviewer receives, whereas
    /// content types are assigned by whichever browser or client did the upload
    /// and are notoriously inconsistent for Office and archive formats. Null
    /// means neither source could establish a format.
    /// </remarks>
    private static string? FormatOf(AttachedDocument document)
    {
        var extension = Path.GetExtension(document.OriginalFileName);

        if (!string.IsNullOrWhiteSpace(extension))
            return Normalize(extension);

        if (string.IsNullOrWhiteSpace(document.ContentType))
            return null;

        // "application/pdf" -> "pdf". Parameters such as "; charset=" are cut
        // first so they never end up inside the subtype.
        var subtype = document.ContentType.Split(';')[0].Split('/').Last();

        return string.IsNullOrWhiteSpace(subtype) ? null : Normalize(subtype);
    }

    private static string Normalize(string value) =>
        value.Trim().TrimStart('.').ToLowerInvariant();
}
