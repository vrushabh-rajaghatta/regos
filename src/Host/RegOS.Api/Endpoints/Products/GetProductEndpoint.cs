using RegOS.Product.Application.Queries.GetProduct;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Products;

public static class GetProductEndpoint
{
    public static IEndpointRouteBuilder MapGetProductEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/products/{id:guid}", HandleAsync);

        return endpoints;
    }

    // No null check and no catch: the handler raises NotFoundException and the
    // middleware maps it to 404, the same as every other capability.
    private static async Task<IResult> HandleAsync(
        Guid id,
        GetProductHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(
            new GetProductQuery(new ProductId(id)),
            cancellationToken);

        return Results.Ok(response);
    }
}
