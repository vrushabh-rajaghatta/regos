using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Application.Commands.ActivateProductDocument;
using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.Api.Endpoints.ProductDocuments;

public static class ActivateProductDocumentEndpoint
{
    public static IEndpointRouteBuilder MapActivateProductDocument(
        this IEndpointRouteBuilder app)
    {
        // Lifecycle transition — an action verb, not a generic PUT/PATCH.
        app.MapPost(
            "/api/products/{globalProductId:guid}/documents/{documentId:guid}/activate",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalProductId,
        Guid documentId,
        ActivateProductDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ActivateProductDocumentCommand(
                new GlobalProductId(globalProductId),
                new ProductDocumentId(documentId)),
            cancellationToken);

        return Results.NoContent();
    }
}
