using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.RegisterProduct;

/// <summary>
/// No organization: a product is registered into the caller's own tenant,
/// which ARCH-002 made ambient.
/// </summary>
public sealed record RegisterProductCommand(
    string? Code,
    string? Name,
    ProductType Type);
