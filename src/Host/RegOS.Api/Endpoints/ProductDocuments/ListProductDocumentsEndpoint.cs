using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Application.Queries.ListProductDocuments;

namespace RegOS.Api.Endpoints.ProductDocuments;

public static class ListProductDocumentsEndpoint
{
    public static IEndpointRouteBuilder MapListProductDocuments(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/products/{productId:guid}/documents",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid productId,
        ListProductDocumentsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ProductId(productId),
            cancellationToken);

        return Results.Ok(result);
    }
}
