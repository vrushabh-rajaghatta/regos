using RegOS.Product.Application.Commands.DeactivateMedicinalProduct;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.MedicinalProducts;

public static class DeactivateMedicinalProductEndpoint
{
    public static IEndpointRouteBuilder MapDeactivateMedicinalProduct(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/medicinal-products/{medicinalProductId:guid}/deactivate",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        MedicinalProductActivationRequest request,
        DeactivateMedicinalProductHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new DeactivateMedicinalProductCommand(
                new MedicinalProductId(medicinalProductId), request.On),
            cancellationToken);

        return Results.NoContent();
    }
}
