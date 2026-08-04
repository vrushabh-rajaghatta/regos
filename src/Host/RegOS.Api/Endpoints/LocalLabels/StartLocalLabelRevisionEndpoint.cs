using RegOS.Labeling.Application.Commands.StartLocalLabelRevision;
using RegOS.Labeling.Domain.Aggregates.LocalLabels;

namespace RegOS.Api.Endpoints.LocalLabels;

public static class StartLocalLabelRevisionEndpoint
{
    public static IEndpointRouteBuilder MapStartLocalLabelRevision(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/local-labels/{localLabelId:guid}/revisions",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid localLabelId,
        StartLocalLabelRevisionHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new StartLocalLabelRevisionCommand(
                LocalLabelId.From(localLabelId)),
            cancellationToken);

        return Results.Created(
            $"/api/local-labels/{localLabelId}/revisions/{result.Id.Value}",
            new LocalLabelRevisionResponse(
                result.Id.Value, result.RevisionNumber));
    }
}
