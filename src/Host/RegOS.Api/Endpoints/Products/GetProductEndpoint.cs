using RegOS.Product.Application.Queries.GetProduct;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Products;

public static class GetProductEndpoint
{
    public static IEndpointRouteBuilder MapGetProductEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "api/products/{id:guid}",
            async (
                Guid id,
                GetProductHandler handler,
                CancellationToken cancellationToken) =>
            {
                var response = await handler.HandleAsync(
                    new GetProductQuery(new ProductId(id)),
                    cancellationToken);

                return response is null
                    ? Results.NotFound()
                    : Results.Ok(response);
            });

        return endpoints;
    }
}