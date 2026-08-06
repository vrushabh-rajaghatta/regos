using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.Submission.Application.Commands.AttachSubmissionToStep;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class AttachSubmissionToStepEndpoint
{
    public static IEndpointRouteBuilder MapAttachSubmissionToStepEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
            "/api/submissions/{id:guid}/process-step", HandleAsync);

        return endpoints;
    }

    /// <summary>
    /// PUT: naming which step a submission contributes to is setting a value, and
    /// sending null clears it. <b>The route lives under submissions</b> — the
    /// aggregate that owns the column owns the command (ADR-065 D2).
    /// </summary>
    private static async Task<IResult> HandleAsync(
        Guid id,
        AttachSubmissionToStepRequest request,
        AttachSubmissionToStepHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AttachSubmissionToStepCommand(
                new SubmissionId(id),
                request.ProcessStepId is { } step
                    ? ProcessStepId.From(step)
                    : null),
            cancellationToken);

        return Results.NoContent();
    }

    public sealed record AttachSubmissionToStepRequest(Guid? ProcessStepId);
}
