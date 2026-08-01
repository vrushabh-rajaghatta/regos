using RegOS.Interaction.Application.Queries.ListInspections;

namespace RegOS.Api.Endpoints.Inspections;

public static class ListInspectionsEndpoint
{
    public static IEndpointRouteBuilder MapListInspections(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/inspections", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListInspectionsHandler handler,
        CancellationToken cancellationToken,
        bool includeConcluded = false)
    {
        var result = await handler.HandleAsync(
            new ListInspectionsQuery(includeConcluded), cancellationToken);

        return Results.Ok(result);
    }
}
