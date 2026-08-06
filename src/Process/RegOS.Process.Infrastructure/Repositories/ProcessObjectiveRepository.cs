using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;

namespace RegOS.Process.Infrastructure.Repositories;

public sealed class ProcessObjectiveRepository : IProcessObjectiveRepository
{
    private readonly RegOSDbContext _dbContext;

    public ProcessObjectiveRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        ProcessObjective objective,
        CancellationToken cancellationToken)
    {
        await _dbContext.ProcessObjectives.AddAsync(objective, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Tracked, with history. The history is auto-included by the shared status
    /// mapping, and it has to be: the chronology rule reads every entry, and an
    /// objective loaded without them would enforce it against an empty
    /// collection and quietly succeed.
    /// </summary>
    public async Task<ProcessObjective?> GetByIdAsync(
        ProcessObjectiveId id,
        CancellationToken cancellationToken)
        => await _dbContext.ProcessObjectives
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(
        ProcessObjective objective,
        CancellationToken cancellationToken)
        => await _dbContext.SaveChangesAsync(cancellationToken);
}
