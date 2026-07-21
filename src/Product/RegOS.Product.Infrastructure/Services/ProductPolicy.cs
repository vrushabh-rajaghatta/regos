using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Infrastructure.Services;

public sealed class ProductPolicy : IProductPolicy
{
    private readonly RegOSDbContext _dbContext;

    public ProductPolicy(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureCodeIsUniqueAsync(
        TenantId tenantId,
        ProductCode code,
        CancellationToken cancellationToken)
    {
        // Whole converted values are compared, never `x.Code.Value` - member
        // access on a value-converted property does not translate to SQL.
        var exists = await _dbContext.Products
            .AnyAsync(
                x => x.TenantId == tenantId && x.Code == code,
                cancellationToken);

        // A duplicate is a state conflict, not a malformed request: the same
        // code would have succeeded a moment earlier. 409 (ADR-009).
        if (exists)
            throw new BusinessRuleViolationException(
                ProductPolicyErrors.CodeAlreadyInUse);
    }
}
