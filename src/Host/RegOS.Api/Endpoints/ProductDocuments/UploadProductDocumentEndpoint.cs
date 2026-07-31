using Microsoft.AspNetCore.Mvc;

using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Application.Commands.UploadProductDocument;
using RegOS.ReferenceData.Domain.DocumentType;

namespace RegOS.Api.Endpoints.ProductDocuments;

public static class UploadProductDocumentEndpoint
{
    public static IEndpointRouteBuilder MapUploadProductDocument(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/products/{globalProductId:guid}/documents",
                HandleAsync)
            // API upload consumed by our SPA; no browser antiforgery token.
            .DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalProductId,
        IFormFile file,
        [FromForm] Guid documentTypeId,
        [FromForm] string name,
        UploadProductDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var result = await handler.HandleAsync(
            new UploadProductDocumentCommand(
                new GlobalProductId(globalProductId),
                new DocumentTypeId(documentTypeId),
                name,
                file.FileName,
                file.ContentType,
                stream),
            cancellationToken);

        return Results.Created(
            $"/api/products/{globalProductId}/documents/{result.Id.Value}",
            new UploadProductDocumentResponse(result.Id.Value));
    }
}
