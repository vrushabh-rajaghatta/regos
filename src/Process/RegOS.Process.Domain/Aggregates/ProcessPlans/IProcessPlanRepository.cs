namespace RegOS.Process.Domain.Aggregates.ProcessPlans;

public interface IProcessPlanRepository
{
    Task AddAsync(ProcessPlan plan, CancellationToken cancellationToken);

    /// <summary>Tracked, with steps and history.</summary>
    Task<ProcessPlan?> GetByIdAsync(
        ProcessPlanId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(ProcessPlan plan, CancellationToken cancellationToken);
}
