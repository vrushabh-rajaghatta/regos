namespace RegOS.Labeling.Domain.Aggregates.GlobalLabels;

public interface IGlobalLabelRepository
{
    Task AddAsync(
        GlobalLabel globalLabel,
        CancellationToken cancellationToken);

    /// <summary>Tracked, with versions — for mutation.</summary>
    Task<GlobalLabel?> GetByIdAsync(
        GlobalLabelId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        GlobalLabel globalLabel,
        CancellationToken cancellationToken);
}
