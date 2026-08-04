namespace RegOS.Labeling.Domain.Aggregates.Contraindications;

public interface IContraindicationRepository
{
    Task AddAsync(Contraindication statement, CancellationToken cancellationToken);

    /// <summary>Tracked, with populations — the rules read the collection.</summary>
    Task<Contraindication?> GetByIdAsync(
        ContraindicationId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(Contraindication statement, CancellationToken cancellationToken);
}
