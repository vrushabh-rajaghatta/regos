using RegOS.Product.Application.Commands.AddPackageItem;
using RegOS.Product.Application.Commands.MovePackageItem;
using RegOS.Product.Application.Commands.RemovePackageItem;
using RegOS.Product.Application.Commands.RestatePackageItem;
using RegOS.Product.Application.Queries.ListPackageItems;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Packs;

/// <summary>
/// The layers of a pack — listed, added, restated, moved and removed.
/// </summary>
/// <remarks>
/// <b>Moving is its own route</b>, not a field on the restate body: where a
/// layer sits is a statement about the tree, checked against every other layer,
/// and folding it into an edit would hide that (ADR-061 §2).
/// </remarks>
public static class PackageItemEndpoints
{
    public static IEndpointRouteBuilder MapPackageItems(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/packaged-products/{packagedProductId:guid}/items", ListAsync);

        app.MapPost(
            "/api/packaged-products/{packagedProductId:guid}/items", AddAsync);

        app.MapPut("/api/package-items/{packageItemId:guid}", RestateAsync);

        app.MapPut("/api/package-items/{packageItemId:guid}/parent", MoveAsync);

        app.MapDelete("/api/package-items/{packageItemId:guid}", RemoveAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(
        Guid packagedProductId,
        ListPackageItemsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListPackageItemsQuery(
                PackagedProductId.From(packagedProductId)),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> AddAsync(
        Guid packagedProductId,
        AddPackageItemRequest request,
        AddPackageItemHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new AddPackageItemCommand(
                PackagedProductId.From(packagedProductId),
                request.ParentPackageItemId is { } parentId
                    ? PackageItemId.From(parentId)
                    : null,
                request.ItemTypeCode,
                request.MaterialCode,
                request.Quantity,
                request.UnitOfPresentationCode,
                request.Description),
            cancellationToken);

        return Results.Created(
            $"/api/package-items/{result.Id.Value}",
            new PackageItemResponse(result.Id.Value));
    }

    private static async Task<IResult> RestateAsync(
        Guid packageItemId,
        RestatePackageItemRequest request,
        RestatePackageItemHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RestatePackageItemCommand(
                PackageItemId.From(packageItemId),
                request.ItemTypeCode,
                request.MaterialCode,
                request.Quantity,
                request.UnitOfPresentationCode,
                request.Description),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> MoveAsync(
        Guid packageItemId,
        MovePackageItemRequest request,
        MovePackageItemHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new MovePackageItemCommand(
                PackageItemId.From(packageItemId),
                request.NewParentPackageItemId is { } parentId
                    ? PackageItemId.From(parentId)
                    : null),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> RemoveAsync(
        Guid packageItemId,
        RemovePackageItemHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RemovePackageItemCommand(PackageItemId.From(packageItemId)),
            cancellationToken);

        return Results.NoContent();
    }
}
