using RegOS.Process.Application.Queries.ListNextSteps;

namespace RegOS.Api.Endpoints.ProcessPlans;

public static class ListNextStepsEndpoint
{
    public static IEndpointRouteBuilder MapListNextStepsEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/process-plans/next-steps", HandleAsync);

        return endpoints;
    }

    /// <summary>
    /// <b>This is the one place "today" is read.</b> The handler takes the date
    /// as a parameter so that every plan read is deterministic and replayable;
    /// supplying it is the endpoint's job, and a caller may override it to ask
    /// what the board looked like on any date.
    /// </summary>
    private static async Task<IResult> HandleAsync(
        DateOnly? asOf,
        ListNextStepsHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(
            new ListNextStepsQuery(
                asOf ?? DateOnly.FromDateTime(DateTime.UtcNow)),
            cancellationToken);

        return Results.Ok(response);
    }
}
