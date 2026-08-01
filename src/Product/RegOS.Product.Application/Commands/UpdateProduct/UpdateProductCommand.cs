using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.UpdateProduct;

/// <summary>
/// Updates a product's descriptive information - name and type only.
/// </summary>
/// <remarks>
/// No organization: the tenant is ambient (ARCH-002). No code: it identifies
/// the product within its organization and is immutable. No status: lifecycle
/// changes are their own capabilities, exactly as Activate and Deactivate are
/// for a user rather than a status field on an update.
/// </remarks>
public sealed record UpdateProductCommand(
    GlobalProductId GlobalProductId,
    string? Name,
    ProductType Type);
