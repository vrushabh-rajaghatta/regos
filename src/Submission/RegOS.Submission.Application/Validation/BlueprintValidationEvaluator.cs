using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.Submission.Application.Validation.Models;
using RegOS.Submission.Application.Validation.Rules;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using IssueSeverity = RegOS.Submission.Application.Validation.Models.ValidationSeverity;

namespace RegOS.Submission.Application.Validation;

/// <summary>
/// Judges a submission against the blueprint it is bound to — the point where
/// reference data governs customer data. Rules are not written in code: they are
/// read from the published template version the submission was pinned to.
/// </summary>
/// <remarks>
/// Orchestration only. It gathers the facts once, then runs two pipelines:
/// checks derived from the blueprint's structure (required-document coverage
/// today, placement completeness later), and the blueprint's explicit
/// <see cref="ValidationRule"/> rows, each handed to whichever
/// <see cref="IBlueprintRuleEvaluator"/> can execute it.
/// <para>
/// A rule no evaluator claims is <em>disclosed</em>, never silently skipped: a
/// regulated engine must be able to say "passed", "failed" and "not evaluated"
/// as three different things.
/// </para>
/// </remarks>
public sealed class BlueprintValidationEvaluator
{
    private readonly RegOSDbContext _dbContext;
    private readonly IReadOnlyList<IBlueprintRuleEvaluator> _ruleEvaluators;
    private readonly RequiredDocumentCoverageEvaluator _coverage = new();
    private readonly UnplacedDocumentEvaluator _unplaced = new();

    public BlueprintValidationEvaluator(RegOSDbContext dbContext)
        : this(dbContext, DefaultRuleEvaluators())
    {
    }

    public BlueprintValidationEvaluator(
        RegOSDbContext dbContext,
        IEnumerable<IBlueprintRuleEvaluator> ruleEvaluators)
    {
        _dbContext = dbContext;
        _ruleEvaluators = ruleEvaluators.ToList();
    }

    /// <summary>
    /// The registry: every rule evaluator this engine can run. Adding a rule
    /// type is one evaluator plus one entry here, and nowhere else.
    /// </summary>
    /// <remarks>
    /// Deliberately the single source of truth. This list and the container's
    /// registrations used to be two lists that had to agree, with nothing making
    /// them — the one place in the architecture that contradicted the "one
    /// evaluator, one registration" story. Composition now reads from here.
    /// Evaluators are stateless functions of (rule, context), so a shared list
    /// of instances is safe.
    /// </remarks>
    public static IReadOnlyList<IBlueprintRuleEvaluator> DefaultRuleEvaluators() =>
        [new FileFormatEvaluator(), new SectionNotEmptyEvaluator()];

    public async Task EvaluateAsync(
        SubmissionAggregate submission,
        SubmissionValidationResult result,
        CancellationToken cancellationToken)
    {
        // An unbound submission is legitimate (no published blueprint targets
        // its application type), but silently skipping the check would be
        // indistinguishable from passing it. Say so, without blocking.
        if (submission.BoundTemplateVersionId is not { } versionId)
        {
            result.AddIssue(
                SubmissionValidationCodes.SubmissionNotBoundToBlueprint,
                "This submission is not bound to a published blueprint, so its "
                    + "completeness against a dossier template was not checked.",
                IssueSeverity.Information);

            return;
        }

        var version = await LoadVersionAsync(versionId, cancellationToken);

        if (version is null)
            return;

        var context = new BlueprintEvaluationContext(
            version,
            await LoadAttachedDocumentsAsync(submission, cancellationToken),
            await LoadDocumentTypeNamesAsync(version, cancellationToken));

        // Two complementary questions about structure: is every placeholder
        // satisfied, and is every document somewhere. Neither subsumes the other.
        _coverage.Evaluate(context, result);
        _unplaced.Evaluate(context, result);

        EvaluateRules(context, result);
    }

    private void EvaluateRules(
        BlueprintEvaluationContext context,
        SubmissionValidationResult result)
    {
        var unevaluated = new List<ValidationRule>();

        foreach (var rule in context.Version.ValidationRules.OrderBy(r => r.Order))
        {
            var evaluator = _ruleEvaluators.FirstOrDefault(e => e.CanEvaluate(rule));

            if (evaluator is null)
            {
                unevaluated.Add(rule);
                continue;
            }

            evaluator.Evaluate(rule, context, result);
        }

        if (unevaluated.Count == 0)
            return;

        // One disclosure, not one per rule. Deliberately phrased as a statement
        // about this engine's capability — it does not say the rules passed, and
        // it does not say they failed, because neither is known. It says nothing
        // about how the blueprint graded them either: reporting "an Error rule
        // was not evaluated" would invite the reader to conclude they have an
        // error, which is exactly what is not known.
        var ruleTypes = unevaluated
            .Select(r => r.RuleType.ToString())
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        result.AddIssue(new SubmissionValidationIssue(
            SubmissionValidationCodes.BlueprintRulesNotEvaluated,
            "This validator does not yet execute these blueprint rule types: "
                + $"{string.Join(", ", ruleTypes)}.",
            IssueSeverity.Information,
            UnevaluatedRuleTypes: ruleTypes));
    }

    /// <summary>
    /// The bound version with everything the evaluators read. The version is a
    /// child of the template aggregate, so it is reached through its root.
    /// </summary>
    private async Task<RegulatoryTemplateVersion?> LoadVersionAsync(
        RegulatoryTemplateVersionId versionId,
        CancellationToken cancellationToken)
    {
        var template = await _dbContext.RegulatoryTemplates
            .AsNoTracking()
            .Include(t => t.Versions)
                // Sections, so issues can name where a document is expected —
                // and, from STORY-003, so a rule can be scoped to one.
                .ThenInclude(v => v.Sections)
            .Include(t => t.Versions)
                .ThenInclude(v => v.RequiredDocuments)
            .Include(t => t.Versions)
                .ThenInclude(v => v.ValidationRules)
            .FirstOrDefaultAsync(
                t => t.Versions.Any(v => v.Id == versionId), cancellationToken);

        return template?.Versions.FirstOrDefault(v => v.Id == versionId);
    }

    private async Task<IReadOnlyList<AttachedDocument>> LoadAttachedDocumentsAsync(
        SubmissionAggregate submission,
        CancellationToken cancellationToken)
    {
        var productDocumentIds = submission.Documents
            .Select(d => d.ProductDocumentId)
            .ToList();

        if (productDocumentIds.Count == 0)
            return [];

        // The attached *version* is what was pinned, so its file facts — not the
        // document's latest — are what the rules are judged against.
        var rows = await _dbContext.ProductDocuments
            .AsNoTracking()
            .Where(d => productDocumentIds.Contains(d.Id))
            .SelectMany(
                d => d.Versions,
                (d, version) => new
                {
                    d.DocumentTypeId,
                    version.Id,
                    version.OriginalFileName,
                    version.ContentType,
                })
            .ToListAsync(cancellationToken);

        // Where each attachment sits. Keyed by the pinned version because that
        // is what the rows above carry; a Product Document may be attached only
        // once per submission, so the key is unique.
        var placementByVersion = submission.Documents
            .ToDictionary(d => d.DocumentVersionId, d => d.TemplateSectionId);

        return rows
            .Where(r => placementByVersion.ContainsKey(r.Id))
            .Select(r => new AttachedDocument(
                r.DocumentTypeId,
                r.OriginalFileName,
                r.ContentType,
                placementByVersion[r.Id]))
            .ToList();
    }

    private async Task<IReadOnlyDictionary<DocumentTypeId, string>>
        LoadDocumentTypeNamesAsync(
            RegulatoryTemplateVersion version,
            CancellationToken cancellationToken)
    {
        var documentTypeIds = version.RequiredDocuments
            .Select(d => d.DocumentTypeId)
            .Distinct()
            .ToList();

        if (documentTypeIds.Count == 0)
            return new Dictionary<DocumentTypeId, string>();

        var rows = await _dbContext.DocumentTypes
            .AsNoTracking()
            .Where(t => documentTypeIds.Contains(t.Id))
            .Select(t => new { t.Id, t.Name })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(r => r.Id, r => r.Name);
    }
}
