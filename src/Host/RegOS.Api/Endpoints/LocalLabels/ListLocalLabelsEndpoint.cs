using RegOS.Labeling.Application.Queries.ListLocalLabels;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.LocalLabels;

public static class ListLocalLabelsEndpoint
{
    public static IEndpointRouteBuilder MapListLocalLabels(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/medicinal-products/{medicinalProductId:guid}/local-labels",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        ListLocalLabelsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListLocalLabelsQuery(new MedicinalProductId(medicinalProductId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
