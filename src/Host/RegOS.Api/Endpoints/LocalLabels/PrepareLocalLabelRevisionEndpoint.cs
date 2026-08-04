using RegOS.Labeling.Application.Commands.PrepareLocalLabelRevision;
using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.ProductDocument.Domain.IDs;

namespace RegOS.Api.Endpoints.LocalLabels;

public static class PrepareLocalLabelRevisionEndpoint
{
    public static IEndpointRouteBuilder MapPrepareLocalLabelRevision(
        this IEndpointRouteBuilder app)
    {
        // PUT: the body is the whole prepared statement, not a patch. These
        // facts are settled together while the revision is being written.
        app.MapPut(
            "/api/local-labels/{localLabelId:guid}/revisions/{revisionId:guid}",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid localLabelId,
        Guid revisionId,
        PrepareLocalLabelRevisionRequest request,
        PrepareLocalLabelRevisionHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new PrepareLocalLabelRevisionCommand(
                LocalLabelId.From(localLabelId),
                LocalLabelRevisionId.From(revisionId),
                request.ContentId is { } content
                    ? new ProductDocumentId(content)
                    : null,
                request.DerivedFromGlobalLabelVersionId is { } derived
                    ? GlobalLabelVersionId.From(derived)
                    : null,
                request.DataCarrierCode,
                request.ChangeSummary),
            cancellationToken);

        return Results.NoContent();
    }
}
