namespace RegOS.Product.Domain.Product;

public interface IMedicinalProductRepository
{
    Task AddAsync(
        MedicinalProduct medicinalProduct,
        CancellationToken cancellationToken);

    /// <summary>
    /// Tracked, with trade names — the aggregate adds to and removes from that
    /// collection, and enforces one-name-per-language across it, so a load
    /// without them would let a duplicate through on any request that did not
    /// happen to be the first.
    /// </summary>
    Task<MedicinalProduct?> GetByIdAsync(
        MedicinalProductId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        MedicinalProduct medicinalProduct,
        CancellationToken cancellationToken);
}
