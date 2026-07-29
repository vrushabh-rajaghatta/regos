using RegOS.ReferenceData.Application.Queries.Blueprint.ListRegulatoryTemplates;

namespace RegOS.Api.Endpoints.ReferenceData;

public static class ListRegulatoryTemplatesEndpoint
{
    public static IEndpointRouteBuilder MapListRegulatoryTemplates(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/reference-data/templates",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListRegulatoryTemplatesHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(cancellationToken);

        return Results.Ok(result);
    }
}
