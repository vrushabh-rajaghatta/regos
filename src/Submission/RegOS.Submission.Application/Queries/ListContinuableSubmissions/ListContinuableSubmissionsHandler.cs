using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Submission.Application.Queries.ListContinuableSubmissions;

public sealed class ListContinuableSubmissionsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListContinuableSubmissionsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// <b>The three filters are the three invariants, read forwards.</b>
    /// <c>Submission.Create</c> refuses an origin that belongs to another
    /// application, is unpublished, or is itself a continuation — so a list
    /// offering any of those would be offering a choice the domain will reject.
    /// The rule is enforced there and merely obeyed here; this query is a
    /// convenience, never the guard.
    /// <para>
    /// A sequence filed before S003 has no <c>SubmissionTypeId</c> and so is
    /// excluded by the join, which is the right outcome for the right reason:
    /// there is no activity to continue, not merely no name to show.
    /// </para>
    /// </remarks>
    public async Task<IReadOnlyList<ContinuableSubmission>> HandleAsync(
        ListContinuableSubmissionsQuery query,
        CancellationToken cancellationToken = default)
    {
        return await (
            from submission in _dbContext.Submissions.AsNoTracking()
            where submission.ApplicationId == query.ApplicationId
                && submission.SequenceNumber != null
                && submission.OriginatingSubmissionId == null
            join submissionType in _dbContext.SubmissionTypes
                on submission.SubmissionTypeId equals submissionType.Id
            orderby submission.SequenceNumber
            select new ContinuableSubmission(
                submission.Id.Value,
                submission.SequenceNumber!.Value,
                submission.Title,
                submissionType.Name))
            .ToListAsync(cancellationToken);
    }
}
