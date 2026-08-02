using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Queries.GetSubmissionChanges;

/// <summary>
/// Reads a filing's frozen operations back out, resolved into names.
/// </summary>
/// <remarks>
/// <b>Nothing is recomputed here.</b> The operations were derived once, at
/// publish, and stored (ADR-045); this query only joins them to the names a
/// person recognises. That is the whole point of freezing them — a view that
/// re-derived the diff would answer with today's rule rather than the one the
/// filing was made under.
/// </remarks>
public sealed class GetSubmissionChangesHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetSubmissionChangesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SubmissionChanges?> HandleAsync(
        GetSubmissionChangesQuery query,
        CancellationToken cancellationToken)
    {
        var submission = await _dbContext.Submissions
            .AsNoTracking()
            .Include(x => x.Documents)
            .Include(x => x.Deletions)
            .FirstOrDefaultAsync(x => x.Id == query.SubmissionId, cancellationToken);

        if (submission is null)
            return null;

        var previousSequenceNumber = submission.SequenceNumber is > 0
            ? submission.SequenceNumber - 1
            : null;

        var operated = submission.Documents
            .Where(d => d.Operation is not null
                        and not SubmissionContentOperation.Unchanged)
            .ToList();

        var unchanged = submission.Documents
            .Count(d => d.Operation == SubmissionContentOperation.Unchanged);

        var names = await ResolveNamesAsync(submission, operated, cancellationToken);

        var changes = operated
            .Select(d => new SubmissionChange(
                d.Operation!.Value.ToString(),
                names.DocumentName(d.ProductDocumentId),
                names.DocumentTypeName(d.ProductDocumentId),
                names.SectionLabel(d.TemplateSectionId!.Value),
                names.VersionNumber(d.DocumentVersionId),
                d.ReplacesSubmissionDocumentId is { } replaced
                    ? names.ReplacedVersionNumber(replaced)
                    : null))
            .Concat(submission.Deletions.Select(x => new SubmissionChange(
                nameof(SubmissionContentOperation.Delete),
                names.DocumentName(x.ProductDocumentId),
                names.DocumentTypeName(x.ProductDocumentId),
                names.SectionLabel(x.TemplateSectionId),
                null,
                names.ReplacedVersionNumber(x.DeletesSubmissionDocumentId))))
            .OrderBy(x => x.SectionLabel, StringComparer.Ordinal)
            .ThenBy(x => x.DocumentName, StringComparer.Ordinal)
            .ToList();

        return new SubmissionChanges(
            submission.SequenceNumber, previousSequenceNumber, changes, unchanged);
    }

    /// <summary>
    /// Every name this view needs, in four reads rather than one per row.
    /// </summary>
    private async Task<Names> ResolveNamesAsync(
        Domain.Submission.Submission submission,
        IReadOnlyCollection<SubmissionDocument> operated,
        CancellationToken cancellationToken)
    {
        var productDocumentIds = operated.Select(d => d.ProductDocumentId)
            .Concat(submission.Deletions.Select(x => x.ProductDocumentId))
            .Distinct()
            .ToList();

        var sectionIds = operated.Select(d => d.TemplateSectionId!.Value)
            .Concat(submission.Deletions.Select(x => x.TemplateSectionId))
            .Distinct()
            .ToList();

        // The version a Replace or Delete points at lives on a *different*
        // submission's placement, so it is looked up by that placement's id.
        var supersededIds = operated
            .Where(d => d.ReplacesSubmissionDocumentId is not null)
            .Select(d => d.ReplacesSubmissionDocumentId!)
            .Concat(submission.Deletions.Select(x => x.DeletesSubmissionDocumentId))
            .Distinct()
            .ToList();

        var documents = await _dbContext.ProductDocuments
            .AsNoTracking()
            .Where(d => productDocumentIds.Contains(d.Id))
            .Select(d => new { d.Id, d.Name, d.DocumentTypeId })
            .ToListAsync(cancellationToken);

        var documentTypes = await _dbContext.DocumentTypes
            .AsNoTracking()
            .Select(t => new { t.Id, t.Name })
            .ToListAsync(cancellationToken);

        var sections = await _dbContext.RegulatoryTemplates
            .AsNoTracking()
            .SelectMany(t => t.Versions)
            .SelectMany(v => v.Sections)
            .Where(s => sectionIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Code, s.Title })
            .ToListAsync(cancellationToken);

        var versions = await _dbContext.Submissions
            .AsNoTracking()
            .SelectMany(s => s.Documents)
            .Where(d => supersededIds.Contains(d.Id))
            .Select(d => new { d.Id, d.DocumentVersionId })
            .ToListAsync(cancellationToken);

        var versionNumbers = await _dbContext.ProductDocuments
            .AsNoTracking()
            .SelectMany(d => d.Versions)
            .Select(v => new { v.Id, v.VersionNumber })
            .ToDictionaryAsync(v => v.Id, v => v.VersionNumber, cancellationToken);

        return new Names(
            documents.ToDictionary(d => d.Id, d => (d.Name, d.DocumentTypeId)),
            documentTypes.ToDictionary(t => t.Id, t => t.Name),
            sections.ToDictionary(s => s.Id, s => $"{s.Code} {s.Title}"),
            versions.ToDictionary(v => v.Id, v => v.DocumentVersionId),
            versionNumbers);
    }

    private sealed record Names(
        Dictionary<ProductDocumentId, (string Name, DocumentTypeId TypeId)> Documents,
        Dictionary<DocumentTypeId, string> DocumentTypes,
        Dictionary<TemplateSectionId, string> Sections,
        Dictionary<SubmissionDocumentId, DocumentVersionId> SupersededVersions,
        Dictionary<DocumentVersionId, int> VersionNumbers)
    {
        public string DocumentName(ProductDocumentId id) =>
            Documents.TryGetValue(id, out var d) ? d.Name : id.Value.ToString();

        public string DocumentTypeName(ProductDocumentId id) =>
            Documents.TryGetValue(id, out var d)
            && DocumentTypes.TryGetValue(d.TypeId, out var name)
                ? name
                : "Unknown";

        public string SectionLabel(TemplateSectionId id) =>
            Sections.TryGetValue(id, out var label) ? label : id.Value.ToString();

        public int? VersionNumber(DocumentVersionId id) =>
            VersionNumbers.TryGetValue(id, out var number) ? number : null;

        public int? ReplacedVersionNumber(SubmissionDocumentId placementId) =>
            SupersededVersions.TryGetValue(placementId, out var versionId)
                ? VersionNumber(versionId)
                : null;
    }
}
