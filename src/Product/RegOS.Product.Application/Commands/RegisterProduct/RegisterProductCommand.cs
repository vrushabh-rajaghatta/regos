using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.RegisterProduct;

public sealed record RegisterProductCommand(string Name, ProductType Type);