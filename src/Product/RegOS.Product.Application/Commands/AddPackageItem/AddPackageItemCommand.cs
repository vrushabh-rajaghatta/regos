using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.AddPackageItem;

/// <param name="ParentPackageItemId">
/// Null for the outermost layer — what a dispenser takes off the shelf; set for
/// something inside it.
/// </param>
public sealed record AddPackageItemCommand(
    PackagedProductId PackagedProductId,
    PackageItemId? ParentPackageItemId,
    string ItemTypeCode,
    string? MaterialCode,
    decimal Quantity,
    string? UnitOfPresentationCode,
    string? Description);
