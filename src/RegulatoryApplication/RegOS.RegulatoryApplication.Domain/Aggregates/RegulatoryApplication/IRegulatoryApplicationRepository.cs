using RegOS.Product.Domain.Product;

namespace RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

public interface IRegulatoryApplicationRepository
{
    Task AddAsync(
        RegulatoryApplication application,
        CancellationToken cancellationToken);

    Task<RegulatoryApplication?> GetByIdAsync(
        RegulatoryApplicationId id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RegulatoryApplication>> ListByProductAsync(
        GlobalProductId globalProductId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists changes to an aggregate this repository loaded.
    /// </summary>
    /// <remarks>
    /// Added when the context gained its first mutation. <c>AddAsync</c> saves
    /// as it adds, which was enough while creation was the only write.
    /// </remarks>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
