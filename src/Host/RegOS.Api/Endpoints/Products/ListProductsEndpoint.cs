using RegOS.Product.Application.Queries.ListProducts;

namespace RegOS.Api.Endpoints.Products;

public static class ListProductsEndpoint
{
    public static IEndpointRouteBuilder MapListProductsEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/products", HandleAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        ListProductsHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(
            new ListProductsQuery(), cancellationToken);

        return Results.Ok(response);
    }
}
