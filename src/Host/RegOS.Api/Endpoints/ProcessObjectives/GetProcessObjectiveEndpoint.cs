using RegOS.Process.Application.Queries.GetProcessObjective;

namespace RegOS.Api.Endpoints.ProcessObjectives;

public static class GetProcessObjectiveEndpoint
{
    public static IEndpointRouteBuilder MapGetProcessObjectiveEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/process-objectives/{id:guid}", HandleAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        GetProcessObjectiveHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(
            new GetProcessObjectiveQuery(id),
            cancellationToken);

        return Results.Ok(response);
    }
}
