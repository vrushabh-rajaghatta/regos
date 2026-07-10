using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.GetProduct;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    ProductType Type,
    ProductStatus Status);