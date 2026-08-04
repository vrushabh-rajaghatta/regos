namespace RegOS.Labeling.Domain.Aggregates.UndesirableEffects;

public interface IUndesirableEffectRepository
{
    Task AddAsync(UndesirableEffect statement, CancellationToken cancellationToken);

    /// <summary>Tracked, with populations — the rules read the collection.</summary>
    Task<UndesirableEffect?> GetByIdAsync(
        UndesirableEffectId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(UndesirableEffect statement, CancellationToken cancellationToken);
}
