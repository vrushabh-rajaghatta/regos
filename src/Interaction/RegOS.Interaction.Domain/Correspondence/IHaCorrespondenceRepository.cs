namespace RegOS.Interaction.Domain.Correspondence;

public interface IHaCorrespondenceRepository
{
    Task AddAsync(
        HaCorrespondence correspondence,
        CancellationToken cancellationToken);

    /// <summary>Tracked — for mutation. Reads go through the DbContext (ADR-016).</summary>
    Task<HaCorrespondence?> GetByIdAsync(
        HaCorrespondenceId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        HaCorrespondence correspondence,
        CancellationToken cancellationToken);
}
