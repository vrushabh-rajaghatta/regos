using RegOS.Process.Application.Queries.GetProcessPlan;

namespace RegOS.Api.Endpoints.ProcessPlans;

public static class GetProcessPlanEndpoint
{
    public static IEndpointRouteBuilder MapGetProcessPlanEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/process-plans/{id:guid}", HandleAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        GetProcessPlanHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(
            new GetProcessPlanQuery(id), cancellationToken);

        return Results.Ok(response);
    }
}
