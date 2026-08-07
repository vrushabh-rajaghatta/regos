using RegOS.Process.Application.Commands.ChangeProcessStepStatus;
using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Api.Endpoints.ProcessPlans;

public static class ChangeProcessStepStatusEndpoint
{
    public static IEndpointRouteBuilder MapChangeProcessStepStatusEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/process-plans/{planId:guid}/steps/{stepId:guid}/status",
            HandleAsync);

        return endpoints;
    }

    /// <summary>
    /// The only way a step's status ever changes. There is deliberately no
    /// endpoint, event handler or job that transitions one on a user's behalf
    /// (ADR-065 D11).
    /// </summary>
    private static async Task<IResult> HandleAsync(
        Guid planId,
        Guid stepId,
        ChangeProcessStepStatusRequest request,
        ChangeProcessStepStatusHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ChangeProcessStepStatusCommand(
                ProcessPlanId.From(planId),
                ProcessStepId.From(stepId),
                request.Status,
                request.OccurredOn,
                request.Note),
            cancellationToken);

        return Results.NoContent();
    }

    public sealed record ChangeProcessStepStatusRequest(
        ProcessStepStatus Status,
        DateOnly OccurredOn,
        string? Note);
}
