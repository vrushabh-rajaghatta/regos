using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Application.Persistence;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Queries.GetProduct;

public sealed class GetProductHandler
{
    private readonly IProductRepository _repository;
    private readonly ITenantContext _tenantContext;

    public GetProductHandler(
        IProductRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<ProductResponse> HandleAsync(
        GetProductQuery query,
        CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(
            query.Id, cancellationToken);

        // A product in another organization is indistinguishable from one that
        // does not exist, so the API never reveals that it is there.
        if (product is null
            || product.OrganizationId != new OrganizationId(_tenantContext.TenantId))
            throw new NotFoundException(ProductQueryErrors.ProductNotFound);

        return new ProductResponse(
            product.Id.Value,
            product.Code.Value,
            product.Name.Value,
            product.Type,
            product.Status);
    }
}
