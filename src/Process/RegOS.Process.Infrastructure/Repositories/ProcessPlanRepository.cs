using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Process.Infrastructure.Repositories;

public sealed class ProcessPlanRepository : IProcessPlanRepository
{
    private readonly RegOSDbContext _dbContext;

    public ProcessPlanRepository(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ProcessPlan plan, CancellationToken cancellationToken)
    {
        await _dbContext.ProcessPlans.AddAsync(plan, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Tracked, with steps and their dependencies. The history is auto-included
    /// by the shared status mapping.
    /// </summary>
    public async Task<ProcessPlan?> GetByIdAsync(
        ProcessPlanId id,
        CancellationToken cancellationToken)
        => await _dbContext.ProcessPlans
            .Include(x => x.Steps)
                .ThenInclude(x => x.Predecessors)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task UpdateAsync(
        ProcessPlan plan,
        CancellationToken cancellationToken)
        => await _dbContext.SaveChangesAsync(cancellationToken);
}
