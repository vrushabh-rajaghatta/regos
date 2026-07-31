using RegOS.Product.Application.Commands.UpdateProduct;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Products;

public static class UpdateProductEndpoint
{
    public static IEndpointRouteBuilder MapUpdateProductEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/products/{id:guid}", HandleAsync)
            .WithName("UpdateProduct")
            .WithSummary("Update a product's descriptive information")
            .WithTags("Product");

        return endpoints;
    }

    // 204: the caller already knows what it sent, and if it wants the refreshed
    // record it can GET the product. Mirrors UpdateUserProfile.
    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateProductRequest request,
        UpdateProductHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new UpdateProductCommand(
                GlobalProductId.From(id), request.Name, request.Type),
            cancellationToken);

        return Results.NoContent();
    }
}
