using RegOS.Product.Application.Persistence;

namespace RegOS.Product.Application.Queries.ListProducts;

public sealed class ListProductsHandler
{
    private readonly IProductRepository _repository;

    public ListProductsHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ProductSummaryResponse>> HandleAsync(
        ListProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        var products = await _repository.ListAsync(cancellationToken);

        return products
            .Select(product => new ProductSummaryResponse(
                product.Id.Value,
                product.Name.Value,
                product.Type,
                product.Status))
            .ToList();
    }
}