using RegOS.Product.Application.Commands.AddPack;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Packs;

public static class AddPackEndpoint
{
    public static IEndpointRouteBuilder MapAddPack(this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/medicinal-products/{medicinalProductId:guid}/packaged-products",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        PackRequest request,
        AddPackHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new AddPackCommand(
                new MedicinalProductId(medicinalProductId),
                request.Description,
                request.PackSizeQuantity,
                request.PackSizeUnitCode,
                request.PackCode,
                request.StatusDate),
            cancellationToken);

        return Results.Created(
            $"/api/packaged-products/{result.Id.Value}",
            new PackResponse(result.Id.Value));
    }
}
