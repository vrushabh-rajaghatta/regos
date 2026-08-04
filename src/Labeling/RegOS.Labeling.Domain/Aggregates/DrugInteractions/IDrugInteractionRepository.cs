namespace RegOS.Labeling.Domain.Aggregates.DrugInteractions;

public interface IDrugInteractionRepository
{
    Task AddAsync(DrugInteraction interaction, CancellationToken cancellationToken);

    /// <summary>Tracked, with interactants — the at-least-one rule reads them.</summary>
    Task<DrugInteraction?> GetByIdAsync(
        DrugInteractionId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(DrugInteraction interaction, CancellationToken cancellationToken);
}
