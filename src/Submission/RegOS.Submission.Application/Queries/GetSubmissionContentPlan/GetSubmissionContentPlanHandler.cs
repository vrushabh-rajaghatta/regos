using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ProductDocument.Domain.Entities;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;
using RegOS.Submission.Domain.Submission;

using ClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.ClinicalStudy.ClinicalStudy;
using NonClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudy;

namespace RegOS.Submission.Application.Queries.GetSubmissionContentPlan;

/// <summary>
/// Builds the dossier tree: the bound blueprint's structure, merged with what
/// the submission has actually placed into it.
/// </summary>
/// <remarks>
/// A read model, assembled from four cheap reads rather than from aggregates.
/// The blueprint is read straight through — nothing is copied onto the
/// submission, because the bound version is immutable and therefore already a
/// snapshot. Placeholder satisfaction is <em>derived</em> here from (section,
/// document type); it is not stored anywhere.
/// </remarks>
public sealed class GetSubmissionContentPlanHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetSubmissionContentPlanHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns the plan, or null when the submission does not exist — so the
    /// endpoint can 404 rather than return an empty dossier for a missing one.
    /// </summary>
    public async Task<SubmissionContentPlan?> HandleAsync(
        SubmissionId submissionId,
        CancellationToken cancellationToken)
    {
        var submission = await _dbContext.Submissions
            .AsNoTracking()
            .Where(s => s.Id == submissionId)
            .Select(s => new { s.Id, s.BoundTemplateVersionId })
            .SingleOrDefaultAsync(cancellationToken);

        if (submission is null)
            return null;

        var documents = await LoadDocumentsAsync(submissionId, cancellationToken);

        if (submission.BoundTemplateVersionId is not { } versionId)
        {
            // No blueprint: there is no structure to place into, so everything
            // attached is unplaced by definition. The client renders "not
            // governed by a template" instead of an error.
            return new SubmissionContentPlan(
                submission.Id.Value,
                BoundTemplateVersionId: null,
                TemplateName: null,
                VersionNumber: null,
                new ContentPlanProgress(0, 0, 0, 0),
                Sections: [],
                UnplacedDocuments: Ordered(documents.Values));
        }

        var blueprint = await LoadBlueprintAsync(versionId, cancellationToken);

        if (blueprint is null)
        {
            // The FK makes this unreachable; treated as "no structure" rather
            // than thrown, because a content plan is a read and a broken
            // reference is not the reader's problem to resolve.
            return new SubmissionContentPlan(
                submission.Id.Value,
                versionId.Value,
                TemplateName: null,
                VersionNumber: null,
                new ContentPlanProgress(0, 0, 0, 0),
                Sections: [],
                UnplacedDocuments: Ordered(documents.Values));
        }

        var (template, version) = blueprint.Value;

        var documentTypeNames = await LoadDocumentTypeNamesAsync(
            version, cancellationToken);

        var placedBySection = documents.Values
            .Where(d => d.SectionId is not null)
            .GroupBy(d => d.SectionId!.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<PlacedDocument>)[.. g]);

        var placeholdersBySection = version.RequiredDocuments
            .GroupBy(r => r.SectionId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<RequiredDocument>)[.. g]);

        var childrenByParent = version.Sections
            .Where(s => s.ParentSectionId is not null)
            .GroupBy(s => s.ParentSectionId!.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<TemplateSection>)[.. g]);

        var roots = version.Sections
            .Where(s => s.ParentSectionId is null)
            .OrderBy(s => s.Order)
            .Select(section => BuildSection(
                section,
                childrenByParent,
                placeholdersBySection,
                placedBySection,
                documentTypeNames))
            .ToList();

        var placeholders = roots.SelectMany(AllPlaceholders).ToList();

        return new SubmissionContentPlan(
            submission.Id.Value,
            versionId.Value,
            template.Name,
            version.VersionNumber,
            new ContentPlanProgress(
                placeholders.Count,
                placeholders.Count(p => p.IsSatisfied),
                placeholders.Count(p => p.IsMandatory),
                placeholders.Count(p => p.IsMandatory && p.IsSatisfied)),
            roots,
            Ordered(documents.Values.Where(d => d.SectionId is null)));
    }

    private static IEnumerable<ContentPlanPlaceholder> AllPlaceholders(
        ContentPlanSection section) =>
        section.Placeholders.Concat(section.Children.SelectMany(AllPlaceholders));

    private static ContentPlanSection BuildSection(
        TemplateSection section,
        IReadOnlyDictionary<TemplateSectionId, IReadOnlyList<TemplateSection>> children,
        IReadOnlyDictionary<TemplateSectionId, IReadOnlyList<RequiredDocument>> placeholders,
        IReadOnlyDictionary<TemplateSectionId, IReadOnlyList<PlacedDocument>> placed,
        IReadOnlyDictionary<DocumentTypeId, string> documentTypeNames)
    {
        var here = placed.TryGetValue(section.Id, out var inSection)
            ? inSection
            : [];

        var expected = placeholders.TryGetValue(section.Id, out var required)
            ? required.OrderBy(r => r.Order).ToList()
            : [];

        // A placeholder is satisfied by a document of its type placed in *this*
        // section. Nothing records that link — it is derived, every time, from
        // the two facts that do exist.
        var built = expected
            .Select(placeholder =>
            {
                var satisfying = here
                    .Where(d => d.DocumentTypeId == placeholder.DocumentTypeId)
                    .ToList();

                return new ContentPlanPlaceholder(
                    placeholder.Id.Value,
                    placeholder.DocumentTypeId.Value,
                    documentTypeNames.TryGetValue(
                        placeholder.DocumentTypeId, out var name)
                        ? name
                        : "Unknown document type",
                    placeholder.IsMandatory,
                    placeholder.Order,
                    satisfying.Count > 0,
                    Ordered(satisfying));
            })
            .ToList();

        var expectedTypes = expected
            .Select(r => r.DocumentTypeId)
            .ToHashSet();

        return new ContentPlanSection(
            section.Id.Value,
            section.Code,
            section.Title,
            section.Order,
            built,
            Ordered(here.Where(d => !expectedTypes.Contains(d.DocumentTypeId))),
            children.TryGetValue(section.Id, out var descendants)
                ? descendants
                    .OrderBy(child => child.Order)
                    .Select(child => BuildSection(
                        child, children, placeholders, placed, documentTypeNames))
                    .ToList()
                : []);
    }

    private static IReadOnlyList<ContentPlanDocument> Ordered(
        IEnumerable<PlacedDocument> documents) =>
        documents
            .OrderBy(d => d.DisplayOrder)
            .Select(d => new ContentPlanDocument(
                d.SubmissionDocumentId,
                d.ProductDocumentId,
                d.Name,
                d.DocumentTypeId.Value,
                d.DocumentTypeName,
                d.VersionNumber,
                d.FileName,
                d.StudyId,
                d.StudyKind,
                d.StudyIdentifier,
                d.FileTag))
            .ToList();

    private async Task<IReadOnlyDictionary<Guid, PlacedDocument>>
        LoadDocumentsAsync(
            SubmissionId submissionId,
            CancellationToken cancellationToken)
    {
        // "SubmissionId" is a shadow FK on the child entity; strongly-typed ids
        // are materialized then unwrapped in memory, as elsewhere in this
        // context (their converters have no SQL translation for .Value).
        var rows = await (
            from attachment in _dbContext.Set<SubmissionDocument>().AsNoTracking()
            where EF.Property<SubmissionId>(attachment, "SubmissionId") == submissionId
            join document in _dbContext.ProductDocuments
                on attachment.ProductDocumentId equals document.Id
            join documentType in _dbContext.DocumentTypes
                on document.DocumentTypeId equals documentType.Id
            join version in _dbContext.Set<DocumentVersion>()
                on attachment.DocumentVersionId equals version.Id
            select new
            {
                attachment.Id,
                attachment.ProductDocumentId,
                attachment.TemplateSectionId,
                attachment.DisplayOrder,
                DocumentName = document.Name,
                document.DocumentTypeId,
                DocumentTypeName = documentType.Name,
                version.VersionNumber,
                version.OriginalFileName,
                attachment.ClinicalStudyId,
                attachment.NonClinicalStudyId,
                attachment.FileTag,
            }).ToListAsync(cancellationToken);

        var studies = await LoadStudyIdentifiersAsync(
            rows.Select(r => r.ClinicalStudyId),
            rows.Select(r => r.NonClinicalStudyId),
            cancellationToken);

        return rows.ToDictionary(
            row => row.Id.Value,
            row =>
            {
                // The exclusive-or, read back: at most one of the two is set,
                // so the first non-null wins and there is nothing to reconcile.
                var studyId = row.ClinicalStudyId?.Value
                    ?? row.NonClinicalStudyId?.Value;

                return new PlacedDocument(
                    row.Id.Value,
                    row.ProductDocumentId.Value,
                    row.TemplateSectionId,
                    row.DisplayOrder,
                    row.DocumentName,
                    row.DocumentTypeId,
                    row.DocumentTypeName,
                    row.VersionNumber,
                    row.OriginalFileName,
                    studyId,
                    row.ClinicalStudyId is not null ? "Clinical"
                        : row.NonClinicalStudyId is not null ? "NonClinical"
                        : null,
                    studyId is { } id && studies.TryGetValue(id, out var code)
                        ? code
                        : null,
                    row.FileTag);
            });
    }

    /// <summary>
    /// The sponsor's code for every study these placements report, in two
    /// round trips rather than two correlated subqueries per row.
    /// </summary>
    /// <remarks>
    /// Two queries because they are two aggregates in two tables (ADR-056).
    /// Merged into one dictionary safely: an identifier names one study across
    /// both kinds, so the guids cannot collide either.
    /// <para>
    /// The <c>Contains</c> is over the <em>typed</em> ids, not their guids: a
    /// strongly typed id's converter has no SQL translation for <c>.Value</c>,
    /// so unwrapping first pushes the whole predicate to client evaluation and
    /// EF refuses to translate it at all.
    /// </para>
    /// </remarks>
    private async Task<IReadOnlyDictionary<Guid, string>>
        LoadStudyIdentifiersAsync(
            IEnumerable<ClinicalStudyId?> clinicalIds,
            IEnumerable<NonClinicalStudyId?> nonClinicalIds,
            CancellationToken cancellationToken)
    {
        var clinical = clinicalIds.OfType<ClinicalStudyId>().Distinct().ToList();

        var nonClinical = nonClinicalIds
            .OfType<NonClinicalStudyId>()
            .Distinct()
            .ToList();

        var identifiers = new Dictionary<Guid, string>();

        if (clinical.Count > 0)
        {
            var rows = await _dbContext.Set<ClinicalStudyAggregate>()
                .AsNoTracking()
                .Where(s => clinical.Contains(s.Id))
                .Select(s => new { s.Id, s.SponsorStudyIdentifier })
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
                identifiers[row.Id.Value] = row.SponsorStudyIdentifier;
        }

        if (nonClinical.Count > 0)
        {
            var rows = await _dbContext.Set<NonClinicalStudyAggregate>()
                .AsNoTracking()
                .Where(s => nonClinical.Contains(s.Id))
                .Select(s => new { s.Id, s.SponsorStudyIdentifier })
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
                identifiers[row.Id.Value] = row.SponsorStudyIdentifier;
        }

        return identifiers;
    }

    /// <summary>
    /// The bound version with its structure. Reached through the aggregate root
    /// so the tenant query filter on templates applies (ADR-031).
    /// </summary>
    private async Task<(RegulatoryTemplate Template, RegulatoryTemplateVersion Version)?>
        LoadBlueprintAsync(
            RegulatoryTemplateVersionId versionId,
            CancellationToken cancellationToken)
    {
        var template = await _dbContext.RegulatoryTemplates
            .AsNoTracking()
            .Include(t => t.Versions)
                .ThenInclude(v => v.Sections)
            .Include(t => t.Versions)
                .ThenInclude(v => v.RequiredDocuments)
            .FirstOrDefaultAsync(
                t => t.Versions.Any(v => v.Id == versionId), cancellationToken);

        var version = template?.Versions.FirstOrDefault(v => v.Id == versionId);

        return version is null ? null : (template!, version);
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

        return await _dbContext.DocumentTypes
            .AsNoTracking()
            .Where(t => documentTypeIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);
    }

    /// <summary>
    /// An attached document, where (if anywhere) it sits, and which study that
    /// placement reports.
    /// </summary>
    private sealed record PlacedDocument(
        Guid SubmissionDocumentId,
        Guid ProductDocumentId,
        TemplateSectionId? SectionId,
        int DisplayOrder,
        string Name,
        DocumentTypeId DocumentTypeId,
        string DocumentTypeName,
        int VersionNumber,
        string FileName,
        Guid? StudyId,
        string? StudyKind,
        string? StudyIdentifier,
        string? FileTag);
}
