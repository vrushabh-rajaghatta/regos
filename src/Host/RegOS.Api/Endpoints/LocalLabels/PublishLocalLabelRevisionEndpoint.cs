using RegOS.Labeling.Application.Commands.PublishLocalLabelRevision;
using RegOS.Labeling.Domain.Aggregates.LocalLabels;

namespace RegOS.Api.Endpoints.LocalLabels;

public static class PublishLocalLabelRevisionEndpoint
{
    public static IEndpointRouteBuilder MapPublishLocalLabelRevision(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/local-labels/{localLabelId:guid}/revisions/{revisionId:guid}/publish",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid localLabelId,
        Guid revisionId,
        PublishLocalLabelRevisionRequest request,
        PublishLocalLabelRevisionHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new PublishLocalLabelRevisionCommand(
                LocalLabelId.From(localLabelId),
                LocalLabelRevisionId.From(revisionId),
                request.ApprovedOn,
                request.EffectiveFrom),
            cancellationToken);

        return Results.NoContent();
    }
}
