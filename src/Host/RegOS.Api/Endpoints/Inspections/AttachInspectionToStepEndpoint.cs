using RegOS.Interaction.Application.Commands.AttachInspectionToStep;
using RegOS.Interaction.Domain.Inspections;
using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Api.Endpoints.Inspections;

public static class AttachInspectionToStepEndpoint
{
    public static IEndpointRouteBuilder MapAttachInspectionToStepEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
            "/api/inspections/{id:guid}/process-step", HandleAsync);

        return endpoints;
    }

    /// <summary>
    /// PUT: naming which step a inspection serves is setting a value, and sending
    /// null clears it. <b>The route lives under inspections</b> — the aggregate
    /// that owns the column owns the command (ADR-065 D2).
    /// </summary>
    private static async Task<IResult> HandleAsync(
        Guid id,
        AttachInspectionToStepRequest request,
        AttachInspectionToStepHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AttachInspectionToStepCommand(
                new InspectionId(id),
                request.ProcessStepId is { } step
                    ? ProcessStepId.From(step)
                    : null),
            cancellationToken);

        return Results.NoContent();
    }

    public sealed record AttachInspectionToStepRequest(Guid? ProcessStepId);
}
