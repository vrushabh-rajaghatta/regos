using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Services;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Infrastructure.Services;

public sealed class SubmissionNumberingPolicy : ISubmissionNumberingPolicy
{
    private readonly RegOSDbContext _dbContext;

    public SubmissionNumberingPolicy(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// Reads through the tenant query filter (ADR-031), so the highest sequence
    /// it can see is the highest sequence this tenant has. That is correct
    /// rather than incidental: an application belongs to one tenant, so there is
    /// no cross-tenant sequence to miss.
    /// </remarks>
    public async Task<NextSequence> GetNextPublishSequenceNumberAsync(
        RegulatoryApplicationId applicationId,
        CancellationToken cancellationToken)
    {
        // Only published submissions have a sequence number at all (ADR-044
        // decision 4), so "the highest number in this application" and "the
        // number of the last thing we filed" are the same question.
        var previous = await _dbContext.Submissions
            .AsNoTracking()
            .Where(x => x.ApplicationId == applicationId
                        && x.SequenceNumber != null)
            .MaxAsync(x => (int?)x.SequenceNumber, cancellationToken);

        return new NextSequence((previous ?? -1) + 1, previous);
    }
}
