using RegOS.Product.Application.Commands.RegisterProduct;

namespace RegOS.Api.Endpoints.Products;

public static class RegisterProductEndpoint
{
    public static IEndpointRouteBuilder MapRegisterProductEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/products", HandleAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        RegisterProductRequest request,
        RegisterProductHandler handler,
        CancellationToken cancellationToken)
    {
        var globalProductId = await handler.HandleAsync(
            new RegisterProductCommand(
                request.Code, request.Name, request.Type),
            cancellationToken);

        return Results.Created(
            $"/api/products/{globalProductId.Value}",
            new RegisterProductResponse(globalProductId.Value));
    }
}
