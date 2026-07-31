using RegOS.Registration.Application.Commands.RecordRegistrationApproval;
using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Api.Endpoints.Registrations;

public static class RecordRegistrationApprovalEndpoint
{
    public static IEndpointRouteBuilder MapRecordRegistrationApproval(
        this IEndpointRouteBuilder app)
    {
        // Recording the grant is the event that makes an authorisation real —
        // a sub-resource of the registration, not an edit to its fields.
        app.MapPost(
            "/registrations/{registrationId:guid}/approval",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid registrationId,
        RecordRegistrationApprovalRequest request,
        RecordRegistrationApprovalHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RecordRegistrationApprovalCommand(
                new RegistrationId(registrationId),
                request.RegistrationNumber,
                request.ApprovedOn,
                request.ExpiresOn,
                request.Note),
            cancellationToken);

        return Results.NoContent();
    }
}
