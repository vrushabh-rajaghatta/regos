namespace RegOS.Process.Domain.Aggregates.ProcessObjectives;

public interface IProcessObjectiveRepository
{
    Task AddAsync(
        ProcessObjective objective,
        CancellationToken cancellationToken);

    /// <summary>Tracked, with history — the chronology rule reads all of it.</summary>
    Task<ProcessObjective?> GetByIdAsync(
        ProcessObjectiveId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        ProcessObjective objective,
        CancellationToken cancellationToken);
}
