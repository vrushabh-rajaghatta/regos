using RegOS.Product.Domain.Product;

namespace RegOS.Product.Domain.Products;

public sealed record RegisterProductRequest(string Name, ProductType Type);