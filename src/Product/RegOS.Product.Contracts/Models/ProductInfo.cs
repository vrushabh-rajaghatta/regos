using RegOS.Product.Domain.Product;

namespace RegOS.Product.Contracts.Models;

public sealed record ProductInfo(
    ProductId Id,
    string Name,
    ProductType Type,
    ProductStatus Status);
