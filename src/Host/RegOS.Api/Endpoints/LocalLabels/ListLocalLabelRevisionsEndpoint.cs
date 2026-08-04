using RegOS.Labeling.Application.Queries.ListLocalLabelRevisions;
using RegOS.Labeling.Domain.Aggregates.LocalLabels;

namespace RegOS.Api.Endpoints.LocalLabels;

public static class ListLocalLabelRevisionsEndpoint
{
    public static IEndpointRouteBuilder MapListLocalLabelRevisions(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/local-labels/{localLabelId:guid}/revisions",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid localLabelId,
        ListLocalLabelRevisionsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListLocalLabelRevisionsQuery(LocalLabelId.From(localLabelId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
