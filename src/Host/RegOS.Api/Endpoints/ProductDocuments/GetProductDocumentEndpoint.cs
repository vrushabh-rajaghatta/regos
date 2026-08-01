using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Application.Queries.GetProductDocument;
using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.Api.Endpoints.ProductDocuments;

public static class GetProductDocumentEndpoint
{
    public static IEndpointRouteBuilder MapGetProductDocument(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/products/{globalProductId:guid}/documents/{documentId:guid}",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalProductId,
        Guid documentId,
        GetProductDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        var document = await handler.HandleAsync(
            new GlobalProductId(globalProductId),
            new ProductDocumentId(documentId),
            cancellationToken);

        return document is null
            ? Results.NotFound()
            : Results.Ok(document);
    }
}
