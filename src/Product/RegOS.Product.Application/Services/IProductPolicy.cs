using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Services;

/// <summary>
/// Rules that span more than one product and so cannot live in the aggregate.
/// Kept intentionally small, like IUserPolicy.
/// </summary>
public interface IProductPolicy
{
    /// <summary>
    /// A product code identifies a product within its owning organization.
    /// Scoped to the tenant, not global: two organizations may legitimately use
    /// the same code for different products.
    /// </summary>
    Task EnsureCodeIsUniqueAsync(
        OrganizationId organizationId,
        ProductCode code,
        CancellationToken cancellationToken);
}
