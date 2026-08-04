using RegOS.Labeling.Application.Commands.DiscardLocalLabelDraft;
using RegOS.Labeling.Domain.Aggregates.LocalLabels;

namespace RegOS.Api.Endpoints.LocalLabels;

public static class DiscardLocalLabelDraftEndpoint
{
    public static IEndpointRouteBuilder MapDiscardLocalLabelDraft(
        this IEndpointRouteBuilder app)
    {
        // DELETE, and only ever of a draft. The aggregate refuses anything an
        // authority has approved, so the verb cannot reach a controlled record.
        app.MapDelete(
            "/api/local-labels/{localLabelId:guid}/draft",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid localLabelId,
        DiscardLocalLabelDraftHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new DiscardLocalLabelDraftCommand(LocalLabelId.From(localLabelId)),
            cancellationToken);

        return Results.NoContent();
    }
}
