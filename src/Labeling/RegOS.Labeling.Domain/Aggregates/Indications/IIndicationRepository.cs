namespace RegOS.Labeling.Domain.Aggregates.Indications;

public interface IIndicationRepository
{
    Task AddAsync(Indication indication, CancellationToken cancellationToken);

    /// <summary>Tracked, with populations, therapies and history — for mutation.</summary>
    Task<Indication?> GetByIdAsync(
        IndicationId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(Indication indication, CancellationToken cancellationToken);
}
