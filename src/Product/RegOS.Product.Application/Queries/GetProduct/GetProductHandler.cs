using RegOS.Product.Application.Persistence;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Queries.GetProduct;

public sealed class GetProductHandler
{
    private readonly IProductRepository _repository;

    public GetProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductResponse> HandleAsync(
        GetProductQuery query,
        CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(
            query.Id, cancellationToken);

        // Queries signal "not found" explicitly rather than returning null, so
        // the API has one contract and nullability does not leak upwards.
        if (product is null)
            throw new NotFoundException(ProductQueryErrors.ProductNotFound);

        return new ProductResponse(
            product.Id.Value,
            product.Name.Value,
            product.Type,
            product.Status);
    }
}
