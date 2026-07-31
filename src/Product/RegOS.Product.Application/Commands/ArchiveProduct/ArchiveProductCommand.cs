using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.ArchiveProduct;

/// <summary>
/// Retires a product from new work. Carries no payload - it is a business
/// decision, not a property update.
/// </summary>
public sealed record ArchiveProductCommand(GlobalProductId GlobalProductId);
