using RegOS.Registration.Application.Commands.ChangeRegistrationStatus;
using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Api.Endpoints.Registrations;

public static class ChangeRegistrationStatusEndpoint
{
    public static IEndpointRouteBuilder MapChangeRegistrationStatus(
        this IEndpointRouteBuilder app)
    {
        // POST rather than PUT: this appends a dated point to an immutable
        // history, it does not overwrite a field. Recording the grant keeps its
        // own endpoint because it carries the number and validity dates — that
        // is a distinct business operation, not a status with extra fields.
        app.MapPost(
            "/registrations/{registrationId:guid}/status",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid registrationId,
        ChangeRegistrationStatusRequest request,
        ChangeRegistrationStatusHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ChangeRegistrationStatusCommand(
                new RegistrationId(registrationId),
                request.Status,
                request.OccurredOn,
                request.Note),
            cancellationToken);

        return Results.NoContent();
    }
}
