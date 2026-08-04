namespace RegOS.Product.Domain.Product;

/// <summary>
/// Writes only (ADR-016). Reads project from <c>RegOSDbContext</c>.
/// </summary>
/// <remarks>
/// <see cref="GetByIdAsync"/> loads the marketing status history with the pack,
/// because <see cref="PackagedProduct.ChangeMarketingStatus"/> compares against
/// it — a guard is only as good as the load, which is the lesson
/// <see cref="IMedicinalProductComponentRepository"/> states one aggregate over.
/// </remarks>
public interface IPackagedProductRepository
{
    Task AddAsync(PackagedProduct pack, CancellationToken cancellationToken);

    Task<PackagedProduct?> GetByIdAsync(
        PackagedProductId id, CancellationToken cancellationToken);

    Task UpdateAsync(PackagedProduct pack, CancellationToken cancellationToken);
}
