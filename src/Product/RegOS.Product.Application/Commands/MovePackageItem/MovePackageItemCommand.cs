using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.MovePackageItem;

/// <param name="NewParentPackageItemId">
/// Null lifts the layer to the outermost level. The subtree travels with it.
/// </param>
public sealed record MovePackageItemCommand(
    PackageItemId PackageItemId,
    PackageItemId? NewParentPackageItemId);
