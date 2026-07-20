using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Products;

public sealed record UpdateProductRequest(string? Name, ProductType Type);
