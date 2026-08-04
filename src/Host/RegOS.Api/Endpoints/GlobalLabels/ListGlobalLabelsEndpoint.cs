using RegOS.Labeling.Application.Queries.ListGlobalLabels;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.GlobalLabels;

public static class ListGlobalLabelsEndpoint
{
    public static IEndpointRouteBuilder MapListGlobalLabels(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/products/{globalProductId:guid}/global-labels",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalProductId,
        ListGlobalLabelsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListGlobalLabelsQuery(new GlobalProductId(globalProductId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
