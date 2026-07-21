using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Product.Application.Queries.GetProduct;

/// <summary>
/// Reads a single product straight from the database: no repository, no
/// aggregate, no tracking. Projects from the flat directory read model so the
/// query stays fully translatable, exactly like the product list and like
/// Platform's user detail query.
/// </summary>
public sealed class GetProductHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetProductHandler(
        RegOSDbContext dbContext,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<ProductDetails> HandleAsync(
        GetProductQuery query,
        CancellationToken cancellationToken)
    {
        var productId = query.Id.Value;
        var tenantId = _tenantContext.TenantId.Value;

        // Tenant isolation: a product in another tenant is
        // indistinguishable from one that does not exist.
        var product = await _dbContext.ProductDirectory
            .AsNoTracking()
            .Where(x => x.Id == productId && x.TenantId == tenantId)
            .Select(x => new ProductDetails(
                x.Id,
                x.Code,
                x.Name,
                x.Type,
                x.Status))
            .SingleOrDefaultAsync(cancellationToken);

        return product
            ?? throw new NotFoundException(ProductQueryErrors.ProductNotFound);
    }
}
