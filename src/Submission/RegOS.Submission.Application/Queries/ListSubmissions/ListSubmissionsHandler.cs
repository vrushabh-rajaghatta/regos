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
            join submissionType in _dbContext.SubmissionTypes
                on submission.SubmissionTypeId equals submissionType.Id
            orderby submission.CreatedOn descending
            select new
            {
                submission.Id,
                submission.Name,
                submission.Status,
                SubmissionTypeName = submissionType.Name,
                submission.CreatedOn,
            }).ToListAsync(cancellationToken);

        return rows
            .Select(row => new SubmissionSummary(
                row.Id.Value,
                row.Name,
                row.Status.ToString(),
                row.SubmissionTypeName,
                row.CreatedOn))
            .ToList();
    }
}
