using Microsoft.EntityFrameworkCore;

using Npgsql;

using RegOS.Persistence;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Infrastructure.Repositories;

public sealed class SubmissionRepository : ISubmissionRepository
{
    /// <summary>
    /// The unique index that makes two submissions in one application unable to
    /// share a sequence number. Named here because this is the one place that
    /// must recognise it; see <see cref="Translate"/>.
    /// </summary>
    private const string SequenceIndex = "IX_Submissions_ApplicationId_SequenceNumber";

    private const string UniqueViolation = "23505";

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
        // Tracked (for mutation) and includes the owned child collections the
        // aggregate reasons over. Roles is not optional: RemoveRole searches it,
        // and AssignRole's duplicate check reads it — an unloaded collection
        // makes the first a silent 404 and the second vacuously true, leaving
        // the unique index to fail what the domain should have refused.
        return await _dbContext.Submissions
            .Include(x => x.Documents)
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(
        SubmissionAggregate submission,
        CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsSequenceCollision(ex))
        {
            throw new SequenceNumberTakenException();
        }
    }

    /// <summary>
    /// Turns one Postgres error into something the application can act on.
    /// </summary>
    /// <remarks>
    /// Deliberately narrow — a single SQLSTATE against a single named index.
    /// Every other unique violation stays a <see cref="DbUpdateException"/>,
    /// because a submission colliding on, say, display order is a defect rather
    /// than a race worth retrying. Knowing an index name is the sort of thing a
    /// repository is allowed to know; the layers above it are not.
    /// </remarks>
    private static bool IsSequenceCollision(DbUpdateException exception)
        => exception.InnerException is PostgresException
        {
            SqlState: UniqueViolation,
            ConstraintName: SequenceIndex
        };
}
