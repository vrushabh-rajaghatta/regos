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

    /// <summary>
    /// All documents placed within the given section <b>or any descendant
    /// section</b> — the shared meaning of "in scope of a section-scoped rule".
    /// </summary>
    /// <remarks>
    /// Deliberately hierarchical, and deliberately different from how
    /// <c>RequiredDocumentCoverageEvaluator</c> matches placeholders. They are
    /// different predicates:
    /// <list type="bullet">
    /// <item>Coverage asks <em>"does this placeholder have a satisfying
    /// document?"</em> — one requirement, in one section, matched exactly.</item>
    /// <item>A section-scoped rule asks <em>"what is in this part of the
    /// dossier?"</em> — a region, which naturally includes what is filed
    /// beneath it.</item>
    /// </list>
    /// A template author writing <c>SectionNotEmpty</c> against <c>3.2.S</c>
    /// means "Drug Substance must contain content", not "a document must be
    /// filed directly on the parent node" — which a well-organised dossier can
    /// never satisfy, because documents live in leaves. For a rule targeting a
    /// leaf, the two readings are identical, which is why every seeded rule
    /// behaves the same either way.
    /// <para>
    /// Anything later that reasons about a section's contents — a maximum
    /// document count, formats allowed in one module — should use this rather
    /// than re-deriving scope, so "in this section" keeps one meaning.
    /// </para>
    /// </remarks>
    public IReadOnlyList<AttachedDocument> DocumentsIn(TemplateSectionId sectionId)
    {
        var scope = SubtreeOf(sectionId);

        return
        [
            .. AttachedDocuments.Where(
                d => d.TemplateSectionId is { } placed && scope.Contains(placed))
        ];
    }

    /// <summary>The section and everything filed beneath it.</summary>
    private HashSet<TemplateSectionId> SubtreeOf(TemplateSectionId root)
    {
        var childrenByParent = Version.Sections
            .Where(s => s.ParentSectionId is not null)
            .ToLookup(s => s.ParentSectionId!.Value);

        var scope = new HashSet<TemplateSectionId> { root };
        var pending = new Queue<TemplateSectionId>([root]);

        while (pending.Count > 0)
        {
            foreach (var child in childrenByParent[pending.Dequeue()])
            {
                // Guarded against a cycle as well as re-visiting: the tree is
                // built by the domain, but a validator should not hang on data.
                if (scope.Add(child.Id))
                    pending.Enqueue(child.Id);
            }
        }

        return scope;
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
