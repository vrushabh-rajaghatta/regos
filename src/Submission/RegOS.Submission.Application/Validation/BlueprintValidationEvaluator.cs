using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.Submission.Application.Validation.Models;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

// Two ValidationSeverity types meet in this file: the blueprint's (how a
// regulatory rule is graded) and the validator's (how an issue affects
// readiness). They are separate concepts in separate contexts, so they are
// named apart rather than merged.
using IssueSeverity = RegOS.Submission.Application.Validation.Models.ValidationSeverity;

namespace RegOS.Submission.Application.Validation;

/// <summary>
/// Judges a submission against the blueprint it is bound to — the point where
/// reference data starts governing customer data. Rules are no longer written
/// in code: they are read from the published template version the submission
/// was pinned to at creation.
/// </summary>
/// <remarks>
/// A collaborator of <see cref="SubmissionValidator"/> rather than more
/// branches inside it, so later capabilities (placement, cardinality, metadata,
/// cross-document checks) arrive as sibling evaluators instead of growing one
/// method.
/// <para>
/// Coverage is answered <em>by document type</em>: "is a document of this type
/// attached?", not "is it in the right section". Placement does not exist until
/// the content plan (EPIC-003), so a type required by two sections is satisfied
/// by one attachment. Today's blueprints require each type once, so nothing is
/// masked — but the limit is real and deliberate.
/// </para>
/// </remarks>
public sealed class BlueprintValidationEvaluator
{
    private readonly RegOSDbContext _dbContext;

    public BlueprintValidationEvaluator(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EvaluateAsync(
        SubmissionAggregate submission,
        SubmissionValidationResult result,
        CancellationToken cancellationToken)
    {
        // An unbound submission is legitimate (no published blueprint targets
        // its submission type), but silently skipping the check would be
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

        var required = await LoadMandatoryDocumentTypesAsync(
            versionId, cancellationToken);

        if (required.Count == 0)
            return;

        var attached = await LoadAttachedDocumentTypesAsync(
            submission, cancellationToken);

        var missing = required.Where(id => !attached.Contains(id)).ToList();

        if (missing.Count == 0)
            return;

        // Name the types rather than reporting bare ids: a validation issue is
        // read by a person deciding what to do next.
        var names = await LoadDocumentTypeNamesAsync(missing, cancellationToken);

        foreach (var documentTypeId in missing)
        {
            var name = names.TryGetValue(documentTypeId, out var found)
                ? found
                : documentTypeId.Value.ToString();

            result.AddIssue(
                SubmissionValidationCodes.RequiredDocumentMissing,
                $"Required document '{name}' is missing.",
                IssueSeverity.Error);
        }
    }

    /// <summary>
    /// The document types the bound version requires, deduplicated. Optional
    /// requirements are excluded: the validator answers "can this proceed?",
    /// not "how could this be improved".
    /// </summary>
    private async Task<IReadOnlyList<DocumentTypeId>> LoadMandatoryDocumentTypesAsync(
        RegulatoryTemplateVersionId versionId,
        CancellationToken cancellationToken)
    {
        // The version is a child of the template aggregate, so it is reached
        // through its root. Small reference data — materialize, then filter in
        // memory rather than fighting LINQ translation.
        var template = await _dbContext.RegulatoryTemplates
            .AsNoTracking()
            .Include(t => t.Versions)
                .ThenInclude(v => v.RequiredDocuments)
            .FirstOrDefaultAsync(
                t => t.Versions.Any(v => v.Id == versionId), cancellationToken);

        var version = template?.Versions.FirstOrDefault(v => v.Id == versionId);

        if (version is null)
            return [];

        return version.RequiredDocuments
            .Where(d => d.IsMandatory)
            .Select(d => d.DocumentTypeId)
            .Distinct()
            .ToList();
    }

    private async Task<HashSet<DocumentTypeId>> LoadAttachedDocumentTypesAsync(
        SubmissionAggregate submission,
        CancellationToken cancellationToken)
    {
        var productDocumentIds = submission.Documents
            .Select(d => d.ProductDocumentId)
            .ToList();

        if (productDocumentIds.Count == 0)
            return [];

        var types = await _dbContext.ProductDocuments
            .AsNoTracking()
            .Where(d => productDocumentIds.Contains(d.Id))
            .Select(d => d.DocumentTypeId)
            .ToListAsync(cancellationToken);

        return [.. types];
    }

    private async Task<Dictionary<DocumentTypeId, string>> LoadDocumentTypeNamesAsync(
        IReadOnlyList<DocumentTypeId> documentTypeIds,
        CancellationToken cancellationToken)
    {
        var rows = await _dbContext.DocumentTypes
            .AsNoTracking()
            .Where(t => documentTypeIds.Contains(t.Id))
            .Select(t => new { t.Id, t.Name })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(r => r.Id, r => r.Name);
    }
}
