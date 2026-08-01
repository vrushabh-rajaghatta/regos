using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.ListMedicinalProducts;

/// <summary>
/// "Which markets is this product in?" — the tier between the product and its
/// licences, listed for the product that owns it.
/// </summary>
public sealed record ListMedicinalProductsQuery(GlobalProductId GlobalProductId);
