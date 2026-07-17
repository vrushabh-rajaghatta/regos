using RegOS.ProductDocument.Domain.IDs;
using RegOS.Submission.Application.Queries.ListProductDocumentUsage;

namespace RegOS.Api.Endpoints.ProductDocuments;

public static class GetProductDocumentUsageEndpoint
{
    public static IEndpointRouteBuilder MapGetProductDocumentUsage(
        this IEndpointRouteBuilder app)
    {
        // Nested under the product document for route consistency; usage is
        // document-scoped, so the query keys off the document id.
        app.MapGet(
            "/api/products/{productId:guid}/documents/{documentId:guid}/usage",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid productId,
        Guid documentId,
        ListProductDocumentUsageHandler handler,
        CancellationToken cancellationToken)
    {
        var usage = await handler.HandleAsync(
            new ProductDocumentId(documentId),
            cancellationToken);

        return Results.Ok(usage);
    }
}
