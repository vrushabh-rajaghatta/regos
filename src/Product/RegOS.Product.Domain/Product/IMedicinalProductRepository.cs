namespace RegOS.Product.Domain.Product;

public interface IMedicinalProductRepository
{
    Task AddAsync(
        MedicinalProduct medicinalProduct,
        CancellationToken cancellationToken);

    /// <summary>
    /// Tracked, with trade names <em>and</em> market-status history — the
    /// aggregate reasons across both. It enforces one-name-per-language over
    /// the first, so a load without it would let a duplicate through on any
    /// request that did not happen to be the first; and it appends to the
    /// second while comparing against its last entry, so a load without that
    /// would silently drop the new entry and lose the chronology check.
    /// </summary>
    Task<MedicinalProduct?> GetByIdAsync(
        MedicinalProductId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        MedicinalProduct medicinalProduct,
        CancellationToken cancellationToken);
}
