using RegOS.Process.Application.Queries.ListProcessObjectives;

namespace RegOS.Api.Endpoints.ProcessObjectives;

public static class ListProcessObjectivesEndpoint
{
    public static IEndpointRouteBuilder MapListProcessObjectivesEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/process-objectives", HandleAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        bool? includeClosed,
        ListProcessObjectivesHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(
            new ListProcessObjectivesQuery(includeClosed ?? false),
            cancellationToken);

        return Results.Ok(response);
    }
}
