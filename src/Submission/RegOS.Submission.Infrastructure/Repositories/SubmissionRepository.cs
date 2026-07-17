using Microsoft.EntityFrameworkCore;

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

    public async Task<SubmissionAggregate?> GetByIdAsync(
        SubmissionId id,
        CancellationToken cancellationToken)
    {
        // Tracked (for mutation) and includes the owned document collection.
        return await _dbContext.Submissions
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(
        SubmissionAggregate submission,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
