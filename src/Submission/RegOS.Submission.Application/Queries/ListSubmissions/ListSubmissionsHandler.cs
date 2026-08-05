using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Submission.Application.Queries.ListSubmissions;

public sealed class ListSubmissionsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListSubmissionsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SubmissionSummary>> HandleAsync(
        RegulatoryApplicationId applicationId,
        CancellationToken cancellationToken)
    {
        // Read model joins each Submission to its (reference-data) type name.
        // Status is mapped to an int column, so ToString() is materialized in
        // memory rather than translated to SQL.
        var rows = await (
            from submission in _dbContext.Submissions.AsNoTracking()
            where submission.ApplicationId == applicationId
            // The application classification, reached through the application
            // that owns it rather than copied onto each sequence (E11, S001).
            join application in _dbContext.RegulatoryApplications
                on submission.ApplicationId equals application.Id
            join applicationType in _dbContext.ApplicationTypes
                on application.ApplicationTypeId equals applicationType.Id
            orderby submission.CreatedOn descending, submission.Id
            select new
            {
                submission.Id,
                submission.Title,
                submission.Status,
                ApplicationTypeName = applicationType.Name,
                submission.Format,
                submission.CreatedOn,
                submission.SequenceNumber,
            }).ToListAsync(cancellationToken);

        return rows
            .Select(row => new SubmissionSummary(
                row.Id.Value,
                row.Title,
                row.Status.ToString(),
                row.ApplicationTypeName,
                row.Format.ToString(),
                row.CreatedOn,
                row.SequenceNumber))
            .ToList();
    }
}
