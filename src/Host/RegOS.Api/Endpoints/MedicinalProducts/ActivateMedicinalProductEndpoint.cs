using RegOS.Product.Application.Commands.ActivateMedicinalProduct;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.MedicinalProducts;

public static class ActivateMedicinalProductEndpoint
{
    public static IEndpointRouteBuilder MapActivateMedicinalProduct(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/medicinal-products/{medicinalProductId:guid}/activate",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        MedicinalProductActivationRequest request,
        ActivateMedicinalProductHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ActivateMedicinalProductCommand(
                new MedicinalProductId(medicinalProductId), request.On),
            cancellationToken);

        return Results.NoContent();
    }
}
