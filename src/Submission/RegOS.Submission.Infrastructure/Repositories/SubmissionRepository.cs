using RegOS.Persistence;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Infrastructure.Repositories;

public sealed class SubmissionRepository : ISubmissionRepository
{
    private readonly RegOSDbContext _dbContext;

    public SubmissionRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        SubmissionAggregate submission,
        CancellationToken cancellationToken)
    {
        _dbContext.Submissions.Add(submission);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
