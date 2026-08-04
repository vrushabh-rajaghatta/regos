namespace RegOS.Product.Domain.Product;

/// <summary>
/// Loads by market, not only by id — because every rule about shape needs the
/// whole tree.
/// </summary>
/// <remarks>
/// <see cref="ListForMarketAsync"/> is not a convenience. A partial list would
/// make a cycle undetectable and a depth check optimistic, so the operations
/// that change the tree take one built from all of it. This is the same lesson
/// as the composition <c>Include</c>, one tier out: the guard is only as good
/// as the load.
/// </remarks>
public interface IMedicinalProductComponentRepository
{
    Task AddAsync(
        MedicinalProductComponent component, CancellationToken cancellationToken);

    Task<MedicinalProductComponent?> GetByIdAsync(
        MedicinalProductComponentId id, CancellationToken cancellationToken);

    /// <summary>
    /// Every component of one market, tracked — the material a
    /// <see cref="ComponentTree"/> is built from.
    /// </summary>
    Task<IReadOnlyList<MedicinalProductComponent>> ListForMarketAsync(
        MedicinalProductId medicinalProductId, CancellationToken cancellationToken);

    Task RemoveAsync(
        MedicinalProductComponent component, CancellationToken cancellationToken);

    Task UpdateAsync(
        MedicinalProductComponent component, CancellationToken cancellationToken);
}
