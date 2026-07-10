using RegOS.Product.Application.Persistence;

namespace RegOS.Product.Application.Queries.GetProduct;

public sealed class GetProductHandler
{
    private readonly IProductRepository _repository;

    public GetProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductResponse?> HandleAsync(
        GetProductQuery query,
        CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(
            query.Id,
            cancellationToken);

        if (product is null)
            return null;

        return new ProductResponse(
            product.Id.Value,
            product.Name.Value,
            product.Type,
            product.Status);
    }
}