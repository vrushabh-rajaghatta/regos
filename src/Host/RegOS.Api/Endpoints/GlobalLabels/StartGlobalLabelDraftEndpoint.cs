using RegOS.Labeling.Application.Commands.StartGlobalLabelDraft;
using RegOS.Labeling.Domain.Aggregates.GlobalLabels;

namespace RegOS.Api.Endpoints.GlobalLabels;

public static class StartGlobalLabelDraftEndpoint
{
    public static IEndpointRouteBuilder MapStartGlobalLabelDraft(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/global-labels/{globalLabelId:guid}/versions",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalLabelId,
        StartGlobalLabelDraftHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new StartGlobalLabelDraftCommand(
                GlobalLabelId.From(globalLabelId)),
            cancellationToken);

        return Results.Created(
            $"/api/global-labels/{globalLabelId}/versions/{result.Id.Value}",
            new GlobalLabelVersionResponse(result.Id.Value, result.VersionNumber));
    }
}
