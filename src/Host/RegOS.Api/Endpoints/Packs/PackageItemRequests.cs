namespace RegOS.Api.Endpoints.Packs;

/// <param name="ParentPackageItemId">
/// Null for the outermost layer — what a dispenser takes off the shelf.
/// </param>
public sealed record AddPackageItemRequest(
    Guid? ParentPackageItemId,
    string ItemTypeCode,
    string? MaterialCode,
    decimal Quantity,
    string? UnitOfPresentationCode,
    string? Description);

/// <remarks>
/// No parent: where a layer sits is a statement about the tree and has its own
/// route.
/// </remarks>
public sealed record RestatePackageItemRequest(
    string ItemTypeCode,
    string? MaterialCode,
    decimal Quantity,
    string? UnitOfPresentationCode,
    string? Description);

public sealed record MovePackageItemRequest(Guid? NewParentPackageItemId);

public sealed record PackageItemResponse(Guid Id);
