using RegOS.Product.Application.Commands.RestatePack;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Packs;

public static class RestatePackEndpoint
{
    public static IEndpointRouteBuilder MapRestatePack(
        this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/packaged-products/{packagedProductId:guid}", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid packagedProductId,
        RestatePackRequest request,
        RestatePackHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RestatePackCommand(
                PackagedProductId.From(packagedProductId),
                request.Description,
                request.PackSizeQuantity,
                request.PackSizeUnitCode,
                request.PackCode),
            cancellationToken);

        return Results.NoContent();
    }
}
