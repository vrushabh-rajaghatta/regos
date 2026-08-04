using RegOS.Labeling.Application.Queries.ListGlobalLabelVersions;
using RegOS.Labeling.Domain.Aggregates.GlobalLabels;

namespace RegOS.Api.Endpoints.GlobalLabels;

public static class ListGlobalLabelVersionsEndpoint
{
    public static IEndpointRouteBuilder MapListGlobalLabelVersions(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/global-labels/{globalLabelId:guid}/versions",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalLabelId,
        ListGlobalLabelVersionsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListGlobalLabelVersionsQuery(
                GlobalLabelId.From(globalLabelId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
