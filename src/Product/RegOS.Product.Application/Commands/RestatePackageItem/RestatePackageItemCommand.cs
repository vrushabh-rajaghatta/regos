using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.RestatePackageItem;

/// <remarks>
/// Everything about the layer except where it sits — moving is its own command,
/// because where a layer sits is a statement about the tree.
/// </remarks>
public sealed record RestatePackageItemCommand(
    PackageItemId PackageItemId,
    string ItemTypeCode,
    string? MaterialCode,
    decimal Quantity,
    string? UnitOfPresentationCode,
    string? Description);
