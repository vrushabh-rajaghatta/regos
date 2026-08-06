using RegOS.Process.Application.Queries.GetPlanImpact;

namespace RegOS.Api.Endpoints.ProcessPlans;

public static class GetPlanImpactEndpoint
{
    public static IEndpointRouteBuilder MapGetPlanImpactEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/process-plans/{id:guid}/impact", HandleAsync);

        return endpoints;
    }

    /// <summary>
    /// An analysis, not a schedule. It never writes, and the response says
    /// "projected" everywhere it gives a date (ADR-065 I7, I8).
    /// </summary>
    private static async Task<IResult> HandleAsync(
        Guid id,
        DateOnly? asOf,
        GetPlanImpactHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(
            new GetPlanImpactQuery(
                id, asOf ?? DateOnly.FromDateTime(DateTime.UtcNow)),
            cancellationToken);

        return Results.Ok(response);
    }
}
