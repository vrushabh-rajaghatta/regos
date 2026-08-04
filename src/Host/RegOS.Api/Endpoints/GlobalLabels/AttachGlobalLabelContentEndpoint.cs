using RegOS.Labeling.Application.Commands.AttachGlobalLabelContent;
using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.Api.Endpoints.GlobalLabels;

public static class AttachGlobalLabelContentEndpoint
{
    public static IEndpointRouteBuilder MapAttachGlobalLabelContent(
        this IEndpointRouteBuilder app)
    {
        // PUT, not POST: a version points at one document, and pointing it at
        // another replaces rather than adds.
        app.MapPut(
            "/api/global-labels/{globalLabelId:guid}/versions/{versionId:guid}/content",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalLabelId,
        Guid versionId,
        AttachGlobalLabelContentRequest request,
        AttachGlobalLabelContentHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AttachGlobalLabelContentCommand(
                GlobalLabelId.From(globalLabelId),
                GlobalLabelVersionId.From(versionId),
                new ProductDocumentId(request.ContentId)),
            cancellationToken);

        return Results.NoContent();
    }
}
