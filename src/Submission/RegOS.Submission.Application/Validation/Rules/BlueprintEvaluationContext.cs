using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;

namespace RegOS.Submission.Application.Validation.Rules;

/// <summary>
/// Everything the evaluators need about a submission and the blueprint version
/// it is bound to, gathered once by the orchestrator.
/// </summary>
/// <remarks>
/// Passing state rather than a <c>DbContext</c> is what keeps each evaluator a
/// pure function over its inputs: they are unit-testable without a database,
/// and the number of queries stays a property of the orchestrator rather than
/// growing with every rule type added.
/// </remarks>
public sealed record BlueprintEvaluationContext(
    RegulatoryTemplateVersion Version,
    IReadOnlyList<AttachedDocument> AttachedDocuments,
    IReadOnlyDictionary<DocumentTypeId, string> DocumentTypeNames)
{
    public string NameFor(DocumentTypeId documentTypeId) =>
        DocumentTypeNames.TryGetValue(documentTypeId, out var name)
            ? name
            : documentTypeId.Value.ToString();

    /// <summary>
    /// A section as a person refers to it — "1.1 Forms". Issues about a
    /// requirement have to say <em>where</em>, now that where is what decides
    /// whether the requirement is met.
    /// </summary>
    public string SectionLabelFor(TemplateSectionId sectionId)
    {
        var section = Version.Sections.FirstOrDefault(s => s.Id == sectionId);

        return section is null
            ? sectionId.Value.ToString()
            : $"{section.Code} {section.Title}";
    }
}

/// <summary>
/// One document attached to the submission, flattened to the facts rules are
/// written about. The file itself is never read — only what was recorded about
/// it when it was uploaded.
/// </summary>
/// <param name="TemplateSectionId">
/// Where it sits in the dossier, or null if it is attached but not placed.
/// Defaults to unplaced so rules that have nothing to do with structure — the
/// format rules — need not mention it.
/// </param>
public sealed record AttachedDocument(
    DocumentTypeId DocumentTypeId,
    string OriginalFileName,
    string ContentType,
    TemplateSectionId? TemplateSectionId = null);
