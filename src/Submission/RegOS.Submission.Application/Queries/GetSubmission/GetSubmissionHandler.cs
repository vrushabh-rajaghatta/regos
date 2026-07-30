using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Queries.GetSubmission;

public sealed class GetSubmissionHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetSubmissionHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SubmissionDetailDto?> HandleAsync(
        SubmissionId submissionId,
        CancellationToken cancellationToken)
    {
        // Single read-model projection joining the submission to its type
        // (reference data) and parent application, both of which contribute
        // display names. Status maps to an int column, so ToString() is
        // materialized in memory rather than translated to SQL.
        var row = await (
            from submission in _dbContext.Submissions.AsNoTracking()
            where submission.Id == submissionId
            join submissionType in _dbContext.SubmissionTypes
                on submission.SubmissionTypeId equals submissionType.Id
            join application in _dbContext.RegulatoryApplications
                on submission.ApplicationId equals application.Id
            select new
            {
                submission.Id,
                submission.Title,
                submission.ApplicationId,
                ApplicationName = application.Name,
                submission.SubmissionTypeId,
                SubmissionTypeName = submissionType.Name,
                submission.Status,
                submission.CreatedOn,
                submission.BoundTemplateVersionId,
            }).SingleOrDefaultAsync(cancellationToken);

        if (row is null)
            return null;

        return new SubmissionDetailDto(
            row.Id.Value,
            row.Title,
            row.ApplicationId.Value,
            row.ApplicationName,
            row.SubmissionTypeId.Value,
            row.SubmissionTypeName,
            row.Status.ToString(),
            row.CreatedOn,
            await LoadBoundTemplateAsync(
                row.BoundTemplateVersionId, cancellationToken));
    }

    /// <summary>
    /// Resolves the names behind a bound template version. A second query
    /// rather than a left join: the binding is usually absent or hits a handful
    /// of cached reference rows, and it keeps the main projection readable.
    /// </summary>
    private async Task<BoundTemplateDto?> LoadBoundTemplateAsync(
        RegulatoryTemplateVersionId? versionId,
        CancellationToken cancellationToken)
    {
        if (versionId is not { } id)
            return null;

        var template = await _dbContext.RegulatoryTemplates
            .AsNoTracking()
            .Include(t => t.Versions)
            .FirstOrDefaultAsync(
                t => t.Versions.Any(v => v.Id == id), cancellationToken);

        if (template is null)
            return null;

        var version = template.Versions.First(v => v.Id == id);

        return new BoundTemplateDto(
            version.Id.Value,
            template.Id.Value,
            template.Code,
            template.Name,
            version.VersionNumber);
    }
}
