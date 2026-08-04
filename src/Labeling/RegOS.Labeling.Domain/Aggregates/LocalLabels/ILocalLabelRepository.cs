namespace RegOS.Labeling.Domain.Aggregates.LocalLabels;

public interface ILocalLabelRepository
{
    Task AddAsync(
        LocalLabel localLabel,
        CancellationToken cancellationToken);

    /// <summary>Tracked, with revisions — for mutation.</summary>
    Task<LocalLabel?> GetByIdAsync(
        LocalLabelId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        LocalLabel localLabel,
        CancellationToken cancellationToken);
}
