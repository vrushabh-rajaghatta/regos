namespace RegOS.Product.Domain.Product;

/// <summary>
/// Loads by pack, not only by id — because every rule about shape needs the
/// whole tree.
/// </summary>
/// <remarks>
/// The same lesson <see cref="IMedicinalProductComponentRepository"/> states one
/// aggregate over: a partial list would make a cycle undetectable and a depth
/// check optimistic, so the operations that change the tree take one built from
/// all of it. **The guard is only as good as the load.**
/// </remarks>
public interface IPackageItemRepository
{
    Task AddAsync(PackageItem item, CancellationToken cancellationToken);

    Task<PackageItem?> GetByIdAsync(
        PackageItemId id, CancellationToken cancellationToken);

    /// <summary>
    /// Every layer of one pack, tracked — the material a
    /// <see cref="PackagingTree"/> is built from.
    /// </summary>
    Task<IReadOnlyList<PackageItem>> ListForPackAsync(
        PackagedProductId packagedProductId, CancellationToken cancellationToken);

    Task RemoveAsync(PackageItem item, CancellationToken cancellationToken);

    Task UpdateAsync(PackageItem item, CancellationToken cancellationToken);
}
