using RegOS.Product.Application.Queries.ListComponents;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Components;

public static class ListComponentsEndpoint
{
    public static IEndpointRouteBuilder MapListComponents(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/medicinal-products/{medicinalProductId:guid}/components",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        ListComponentsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListComponentsQuery(new MedicinalProductId(medicinalProductId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
