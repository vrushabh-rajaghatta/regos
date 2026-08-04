using RegOS.Labeling.Application.Commands.DiscardGlobalLabelDraft;
using RegOS.Labeling.Domain.Aggregates.GlobalLabels;

namespace RegOS.Api.Endpoints.GlobalLabels;

public static class DiscardGlobalLabelDraftEndpoint
{
    public static IEndpointRouteBuilder MapDiscardGlobalLabelDraft(
        this IEndpointRouteBuilder app)
    {
        // DELETE, and only ever of a draft. The aggregate refuses anything that
        // has been in force, so the verb cannot reach a regulatory record.
        app.MapDelete(
            "/api/global-labels/{globalLabelId:guid}/draft",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalLabelId,
        DiscardGlobalLabelDraftHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new DiscardGlobalLabelDraftCommand(
                GlobalLabelId.From(globalLabelId)),
            cancellationToken);

        return Results.NoContent();
    }
}
