using RegOS.Process.Application.Queries.ListProcessDefinitions;

namespace RegOS.Api.Endpoints.ProcessDefinitions;

public static class ListProcessDefinitionsEndpoint
{
    public static IEndpointRouteBuilder MapListProcessDefinitionsEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/process-definitions", HandleAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        bool? includeRetired,
        ListProcessDefinitionsHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(
            new ListProcessDefinitionsQuery(includeRetired ?? false),
            cancellationToken);

        return Results.Ok(response);
    }
}
