namespace RegOS.Process.Domain.Aggregates.ProcessDefinitions;

/// <summary>
/// Writes go through here; reads use <c>RegOSDbContext</c> with
/// <c>AsNoTracking()</c> (ADR-016, SC-002).
/// </summary>
public interface IProcessDefinitionRepository
{
    Task AddAsync(
        ProcessDefinition definition,
        CancellationToken cancellationToken);

    /// <summary>Tracked, with versions and their steps — for mutation.</summary>
    Task<ProcessDefinition?> GetByIdAsync(
        ProcessDefinitionId id,
        CancellationToken cancellationToken);

    /// <summary>
    /// Used by the seed to stay idempotent: a playbook is identified by its code
    /// within a tenant, and re-running must not create a second one.
    /// </summary>
    Task<bool> ExistsAsync(
        string code,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        ProcessDefinition definition,
        CancellationToken cancellationToken);
}
