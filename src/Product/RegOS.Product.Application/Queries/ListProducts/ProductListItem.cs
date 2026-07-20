using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.ListProducts;

/// <summary>
/// A row in the product directory — a read-only projection optimized for
/// browsing, deliberately NOT the Product aggregate. Exposes only what the list
/// screen needs: no OrganizationId (the caller's tenant is implicit), no value
/// objects, no behaviour.
/// </summary>
public sealed record ProductListItem(
    Guid Id,
    string Code,
    string Name,
    ProductType Type,
    ProductStatus Status);
