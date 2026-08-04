using RegOS.Labeling.Application.Commands.PublishGlobalLabelVersion;
using RegOS.Labeling.Domain.Aggregates.GlobalLabels;

namespace RegOS.Api.Endpoints.GlobalLabels;

public static class PublishGlobalLabelVersionEndpoint
{
    public static IEndpointRouteBuilder MapPublishGlobalLabelVersion(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/global-labels/{globalLabelId:guid}/versions/{versionId:guid}/publish",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalLabelId,
        Guid versionId,
        PublishGlobalLabelVersionRequest request,
        PublishGlobalLabelVersionHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new PublishGlobalLabelVersionCommand(
                GlobalLabelId.From(globalLabelId),
                GlobalLabelVersionId.From(versionId),
                request.EffectiveFrom,
                request.ChangeSummary),
            cancellationToken);

        return Results.NoContent();
    }
}
