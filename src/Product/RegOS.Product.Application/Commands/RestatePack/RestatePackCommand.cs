using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.RestatePack;

/// <remarks>
/// Restates the three facts together rather than patching one: a corrected pack
/// size that left the description saying <em>"carton of 30"</em> would be a pack
/// contradicting itself.
/// </remarks>
public sealed record RestatePackCommand(
    PackagedProductId PackagedProductId,
    string Description,
    decimal? PackSizeQuantity,
    string? PackSizeUnitCode,
    string? PackCode);
