namespace RegOS.Product.Domain.Product;

/// <summary>
/// Its own repository, because it is its own consistency boundary — a
/// presentation is restated without loading the market's trade names, its
/// commercial history or its licences.
/// </summary>
public interface IPharmaceuticalProductDetailRepository
{
    Task AddAsync(
        PharmaceuticalProductDetail detail,
        CancellationToken cancellationToken);

    /// <summary>
    /// Tracked, <b>with its routes</b> — <c>Restate</c> replaces the collection
    /// wholesale, so a load without them would append to an empty list and lose
    /// every route the presentation already had.
    /// </summary>
    Task<PharmaceuticalProductDetail?> GetByIdAsync(
        PharmaceuticalProductDetailId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        PharmaceuticalProductDetail detail,
        CancellationToken cancellationToken);
}
