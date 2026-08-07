using RegOS.Process.Application.Queries.ListObjectivePlans;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;

namespace RegOS.Api.Endpoints.ProcessPlans;

public static class ListObjectivePlansEndpoint
{
    public static IEndpointRouteBuilder MapListObjectivePlansEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/process-objectives/{objectiveId:guid}/plans", HandleAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid objectiveId,
        ListObjectivePlansHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(
            new ListObjectivePlansQuery(ProcessObjectiveId.From(objectiveId)),
            cancellationToken);

        return Results.Ok(response);
    }
}
