using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessDefinitions;

namespace RegOS.Process.Infrastructure.Repositories;

public sealed class ProcessDefinitionRepository : IProcessDefinitionRepository
{
    private readonly RegOSDbContext _dbContext;

    public ProcessDefinitionRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        ProcessDefinition definition,
        CancellationToken cancellationToken)
    {
        await _dbContext.ProcessDefinitions.AddAsync(definition, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Tracked, with versions <em>and</em> their steps — always, never on request.
    /// </summary>
    /// <remarks>
    /// Every rule on this aggregate is a statement about a set. At most one open
    /// draft, the next number is one past the highest, a step code is unique
    /// within its version, a predecessor must belong to the same version, and the
    /// publish-time cycle check reads the whole graph. A playbook loaded without
    /// its steps would enforce all of them against empty collections and quietly
    /// succeed — and the last one would certify a schedule it never looked at.
    /// </remarks>
    public async Task<ProcessDefinition?> GetByIdAsync(
        ProcessDefinitionId id,
        CancellationToken cancellationToken)
        => await _dbContext.ProcessDefinitions
            .Include(x => x.Versions)
                .ThenInclude(x => x.Steps)
                .ThenInclude(x => x.Predecessors)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> ExistsAsync(
        string code,
        CancellationToken cancellationToken)
        => await _dbContext.ProcessDefinitions
            .AsNoTracking()
            .AnyAsync(x => x.Code == code, cancellationToken);

    public async Task UpdateAsync(
        ProcessDefinition definition,
        CancellationToken cancellationToken)
        => await _dbContext.SaveChangesAsync(cancellationToken);
}
