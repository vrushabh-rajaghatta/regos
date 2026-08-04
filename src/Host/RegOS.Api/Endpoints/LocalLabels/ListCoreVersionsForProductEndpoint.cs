using RegOS.Labeling.Application.Queries.ListCoreVersionsForProduct;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.LocalLabels;

public static class ListCoreVersionsForProductEndpoint
{
    public static IEndpointRouteBuilder MapListCoreVersionsForProduct(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/products/{globalProductId:guid}/core-versions",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalProductId,
        ListCoreVersionsForProductHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListCoreVersionsForProductQuery(
                new GlobalProductId(globalProductId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
