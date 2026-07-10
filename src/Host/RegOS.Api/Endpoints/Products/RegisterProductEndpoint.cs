using RegOS.Product.Application.Commands.RegisterProduct;
using RegOS.Product.Domain.Products;

namespace RegOS.Api.Endpoints.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("api/products", async (
            RegisterProductRequest request,
            RegisterProductHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterProductCommand(
                request.Name,
                request.Type);

            var productId = await handler.HandleAsync(command, cancellationToken);

            return Results.Created(
                    $"/api/products/{productId.Value}",
                    new { id = productId.Value });
        });
        return endpoints;
    }
}