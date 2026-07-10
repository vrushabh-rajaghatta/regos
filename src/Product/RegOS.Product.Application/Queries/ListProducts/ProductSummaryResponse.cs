using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.ListProducts;

public sealed record ProductSummaryResponse(
    Guid Id,
    string Name,
    ProductType Type,
    ProductStatus Status);