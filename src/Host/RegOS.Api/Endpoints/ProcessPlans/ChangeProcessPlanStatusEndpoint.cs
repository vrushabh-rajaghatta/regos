using RegOS.Process.Application.Commands.ChangeProcessPlanStatus;
using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Api.Endpoints.ProcessPlans;

public static class ChangeProcessPlanStatusEndpoint
{
    public static IEndpointRouteBuilder MapChangeProcessPlanStatusEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/process-plans/{id:guid}/status", HandleAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        ChangeProcessPlanStatusRequest request,
        ChangeProcessPlanStatusHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ChangeProcessPlanStatusCommand(
                ProcessPlanId.From(id),
                request.Status,
                request.OccurredOn,
                request.Note),
            cancellationToken);

        return Results.NoContent();
    }

    public sealed record ChangeProcessPlanStatusRequest(
        ProcessPlanStatus Status,
        DateOnly OccurredOn,
        string? Note);
}
