using RegOS.Registration.Application.Queries.GetRegistration;
using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Api.Endpoints.Registrations;

public static class GetRegistrationEndpoint
{
    public static IEndpointRouteBuilder MapGetRegistration(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/registrations/{registrationId:guid}", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid registrationId,
        GetRegistrationHandler handler,
        CancellationToken cancellationToken)
    {
        var registration = await handler.HandleAsync(
            new RegistrationId(registrationId),
            cancellationToken);

        return registration is null
            ? Results.NotFound()
            : Results.Ok(registration);
    }
}
