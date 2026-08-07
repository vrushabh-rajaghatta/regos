using RegOS.Interaction.Application.Commands.AttachCommitmentToStep;
using RegOS.Interaction.Domain.Commitments;
using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Api.Endpoints.Commitments;

public static class AttachCommitmentToStepEndpoint
{
    public static IEndpointRouteBuilder MapAttachCommitmentToStepEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
            "/api/commitments/{id:guid}/process-step", HandleAsync);

        return endpoints;
    }

    /// <summary>
    /// PUT: naming which step a commitment serves is setting a value, and sending
    /// null clears it. <b>The route lives under commitments</b> — the aggregate
    /// that owns the column owns the command (ADR-065 D2).
    /// </summary>
    private static async Task<IResult> HandleAsync(
        Guid id,
        AttachCommitmentToStepRequest request,
        AttachCommitmentToStepHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AttachCommitmentToStepCommand(
                new CommitmentId(id),
                request.ProcessStepId is { } step
                    ? ProcessStepId.From(step)
                    : null),
            cancellationToken);

        return Results.NoContent();
    }

    public sealed record AttachCommitmentToStepRequest(Guid? ProcessStepId);
}
