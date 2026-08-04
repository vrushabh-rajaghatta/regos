namespace RegOS.Product.Application.Queries.ListPackageItems;

/// <param name="Depth">
/// One for the outermost layer. Computed by the same tree the rules use, so a
/// row's indentation on screen and the depth the guard measured cannot drift
/// apart.
/// </param>
/// <param name="MaterialDisplay">
/// Null is ordinary — an outer carton's board grade is rarely stated, while a
/// blister's laminate always is.
/// </param>
public sealed record PackageItemSummary(
    Guid Id,
    Guid? ParentPackageItemId,
    int Depth,
    string ItemTypeCode,
    string ItemTypeDisplay,
    string ItemTypeSystem,
    string? MaterialCode,
    string? MaterialDisplay,
    decimal Quantity,
    string? UnitOfPresentationCode,
    string? UnitOfPresentationDisplay,
    string? Description);
