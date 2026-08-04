using RegOS.ReferenceData.Application.Queries.Substances.ListSubstances;

namespace RegOS.Api.Endpoints.Substances;

public static class ListSubstancesEndpoint
{
    public static IEndpointRouteBuilder MapListSubstances(
        this IEndpointRouteBuilder app)
    {
        // One list over both halves of the catalogue, not /shared and /mine:
        // they are the same directory, and which half a row came from is a
        // property of the row rather than a different question.
        app.MapGet("/api/substances", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListSubstancesHandler handler,
        CancellationToken cancellationToken,
        string? search = null,
        SubstanceOrigin origin = SubstanceOrigin.Any)
    {
        var result = await handler.HandleAsync(
            new ListSubstancesQuery(search, origin), cancellationToken);

        return Results.Ok(result);
    }
}
