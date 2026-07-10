namespace RegOS.Product.Application.Contracts;

using RegOS.Product.Domain.Product;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
}