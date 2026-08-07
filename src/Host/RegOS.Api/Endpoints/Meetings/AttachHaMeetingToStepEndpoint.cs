using RegOS.Interaction.Application.Commands.AttachHaMeetingToStep;
using RegOS.Interaction.Domain.Meetings;
using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Api.Endpoints.Meetings;

public static class AttachHaMeetingToStepEndpoint
{
    public static IEndpointRouteBuilder MapAttachHaMeetingToStepEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
            "/api/meetings/{id:guid}/process-step", HandleAsync);

        return endpoints;
    }

    /// <summary>
    /// PUT: naming which step a meeting serves is setting a value, and sending
    /// null clears it. <b>The route lives under meetings</b> — the aggregate
    /// that owns the column owns the command (ADR-065 D2).
    /// </summary>
    private static async Task<IResult> HandleAsync(
        Guid id,
        AttachHaMeetingToStepRequest request,
        AttachHaMeetingToStepHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AttachHaMeetingToStepCommand(
                new HaMeetingId(id),
                request.ProcessStepId is { } step
                    ? ProcessStepId.From(step)
                    : null),
            cancellationToken);

        return Results.NoContent();
    }

    public sealed record AttachHaMeetingToStepRequest(Guid? ProcessStepId);
}
