using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.GetProduct;

/// <summary>
/// Everything the product detail screen shows. Identical in shape to
/// <c>ProductListItem</c> today, and deliberately a separate type: the two
/// screens answer different questions and will diverge (a detail view gains
/// registration dates, documents and applications long before a list row does).
/// Platform does the same with UserListItem and UserDetails.
/// </summary>
public sealed record ProductDetails(
    Guid Id,
    string Code,
    string Name,
    ProductType Type,
    ProductStatus Status);
