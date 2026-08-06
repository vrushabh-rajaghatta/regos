using RegOS.Interaction.Application.Commands.AttachHaCorrespondenceToStep;
using RegOS.Interaction.Domain.Correspondence;
using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Api.Endpoints.Correspondence;

public static class AttachHaCorrespondenceToStepEndpoint
{
    public static IEndpointRouteBuilder MapAttachHaCorrespondenceToStepEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
            "/api/correspondence/{id:guid}/process-step", HandleAsync);

        return endpoints;
    }

    /// <summary>
    /// PUT: naming which step a letter serves is setting a value, and sending
    /// null clears it. <b>The route lives under correspondence</b> — the aggregate
    /// that owns the column owns the command (ADR-065 D2).
    /// </summary>
    private static async Task<IResult> HandleAsync(
        Guid id,
        AttachHaCorrespondenceToStepRequest request,
        AttachHaCorrespondenceToStepHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AttachHaCorrespondenceToStepCommand(
                new HaCorrespondenceId(id),
                request.ProcessStepId is { } step
                    ? ProcessStepId.From(step)
                    : null),
            cancellationToken);

        return Results.NoContent();
    }

    public sealed record AttachHaCorrespondenceToStepRequest(Guid? ProcessStepId);
}
