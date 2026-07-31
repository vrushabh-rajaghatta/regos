namespace RegOS.Product.Domain.Product;

public interface IMedicinalProductRepository
{
    Task AddAsync(
        MedicinalProduct medicinalProduct,
        CancellationToken cancellationToken);

    Task<MedicinalProduct?> GetByIdAsync(
        MedicinalProductId id,
        CancellationToken cancellationToken);
}
