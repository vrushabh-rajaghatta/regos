using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Products;

public sealed record RegisterProductRequest(string Name, ProductType Type);
