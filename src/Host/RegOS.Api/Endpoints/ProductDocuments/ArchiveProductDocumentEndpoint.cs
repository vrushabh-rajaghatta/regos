using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Application.Commands.ArchiveProductDocument;
using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.Api.Endpoints.ProductDocuments;

public static class ArchiveProductDocumentEndpoint
{
    public static IEndpointRouteBuilder MapArchiveProductDocument(
        this IEndpointRouteBuilder app)
    {
        // Lifecycle transition — an action verb, not a generic PUT/PATCH.
        app.MapPost(
            "/api/products/{globalProductId:guid}/documents/{documentId:guid}/archive",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalProductId,
        Guid documentId,
        ArchiveProductDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ArchiveProductDocumentCommand(
                new GlobalProductId(globalProductId),
                new ProductDocumentId(documentId)),
            cancellationToken);

        return Results.NoContent();
    }
}
