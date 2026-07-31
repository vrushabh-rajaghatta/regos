using RegOS.Product.Application.Commands.ArchiveProduct;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Products;

public static class ArchiveProductEndpoint
{
    public static IEndpointRouteBuilder MapArchiveProductEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        // A named lifecycle action, not a status field on a PUT - the same
        // shape as activating and deactivating a user.
        endpoints.MapPost("/api/products/{id:guid}/archive", HandleAsync)
            .WithName("ArchiveProduct")
            .WithSummary("Retire a product from new work")
            .WithTags("Product");

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        ArchiveProductHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ArchiveProductCommand(GlobalProductId.From(id)), cancellationToken);

        return Results.NoContent();
    }
}
