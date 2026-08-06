using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.Registration.Application.Commands.AttachRegistrationToStep;
using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Api.Endpoints.Registrations;

public static class AttachRegistrationToStepEndpoint
{
    public static IEndpointRouteBuilder MapAttachRegistrationToStepEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
            "/api/registrations/{id:guid}/process-step", HandleAsync);

        return endpoints;
    }

    /// <summary>
    /// PUT: naming which step a registration contributes to is setting a value, and
    /// sending null clears it. <b>The route lives under registrations</b> — the
    /// aggregate that owns the column owns the command (ADR-065 D2).
    /// </summary>
    private static async Task<IResult> HandleAsync(
        Guid id,
        AttachRegistrationToStepRequest request,
        AttachRegistrationToStepHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AttachRegistrationToStepCommand(
                new RegistrationId(id),
                request.ProcessStepId is { } step
                    ? ProcessStepId.From(step)
                    : null),
            cancellationToken);

        return Results.NoContent();
    }

    public sealed record AttachRegistrationToStepRequest(Guid? ProcessStepId);
}
