using RegOS.ProductDocument.Application.Commands.UploadDocumentVersion;
using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.Api.Endpoints.ProductDocuments;

public static class UploadDocumentVersionEndpoint
{
    public static IEndpointRouteBuilder MapUploadDocumentVersion(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/products/{globalProductId:guid}/documents/{documentId:guid}/versions",
                HandleAsync)
            // API upload consumed by our SPA; no browser antiforgery token.
            .DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalProductId,
        Guid documentId,
        IFormFile file,
        UploadDocumentVersionHandler handler,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var result = await handler.HandleAsync(
            new UploadDocumentVersionCommand(
                new ProductDocumentId(documentId),
                file.FileName,
                file.ContentType,
                stream),
            cancellationToken);

        return Results.Created(
            $"/api/products/{globalProductId}/documents/{documentId}",
            new UploadDocumentVersionResponse(result.Id.Value, result.VersionNumber));
    }
}
