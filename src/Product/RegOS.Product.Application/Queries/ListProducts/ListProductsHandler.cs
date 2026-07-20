using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Application.Persistence;
using RegOS.SharedKernel.Abstractions;

namespace RegOS.Product.Application.Queries.ListProducts;

public sealed class ListProductsHandler
{
    private readonly IProductRepository _repository;
    private readonly ITenantContext _tenantContext;

    public ListProductsHandler(
        IProductRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<ProductSummaryResponse>> HandleAsync(
        ListProductsQuery query,
        CancellationToken cancellationToken)
    {
        // Tenant filter applied in the repository, unconditionally - there is
        // no call that returns another organization's products.
        var products = await _repository.ListAsync(
            new OrganizationId(_tenantContext.TenantId), cancellationToken);

        return products
            .Select(product => new ProductSummaryResponse(
                product.Id.Value,
                product.Code.Value,
                product.Name.Value,
                product.Type,
                product.Status))
            .ToList();
    }
}
