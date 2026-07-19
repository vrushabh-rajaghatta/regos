using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Submission.Domain.Snapshot;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Infrastructure.Repositories;

public sealed class SubmissionSnapshotRepository : ISubmissionSnapshotRepository
{
    private readonly RegOSDbContext _dbContext;

    public SubmissionSnapshotRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Stages the snapshot for insertion without saving. Snapshots are only ever
    /// created while publishing a submission, so the publish handler owns the unit
    /// of work: it stages the snapshot here and commits it together with the
    /// submission's transition in a single <c>SaveChanges</c>, keeping publish atomic.
    /// </summary>
    public Task AddAsync(
        SubmissionSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        _dbContext.SubmissionSnapshots.Add(snapshot);
        return Task.CompletedTask;
    }

    public async Task<SubmissionSnapshot?> GetByIdAsync(
        SubmissionSnapshotId id,
        CancellationToken cancellationToken)
    {
        // The aggregate boundary includes its documents — never load one without them.
        return await _dbContext.SubmissionSnapshots
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<SubmissionSnapshot?> GetBySubmissionIdAsync(
        SubmissionId submissionId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.SubmissionSnapshots
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.SubmissionId == submissionId, cancellationToken);
    }
}
