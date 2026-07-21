using RegOS.Product.Domain.Product;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Product.Application.Services;

/// <summary>
/// Rules that span more than one product and so cannot live in the aggregate.
/// Kept intentionally small, like IUserPolicy.
/// </summary>
public interface IProductPolicy
{
    /// <summary>
    /// A product code identifies a product within its owning tenant.
    /// Scoped to the tenant, not global: two tenants may legitimately use
    /// the same code for different products.
    /// </summary>
    Task EnsureCodeIsUniqueAsync(
        TenantId tenantId,
        ProductCode code,
        CancellationToken cancellationToken);
}
