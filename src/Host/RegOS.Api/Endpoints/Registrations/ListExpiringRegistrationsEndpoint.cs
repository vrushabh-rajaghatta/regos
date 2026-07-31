using RegOS.Registration.Application.Queries.ListExpiringRegistrations;

namespace RegOS.Api.Endpoints.Registrations;

public static class ListExpiringRegistrationsEndpoint
{
    public static IEndpointRouteBuilder MapListExpiringRegistrations(
        this IEndpointRouteBuilder app)
    {
        // Every registration still on the validity timeline, nearest expiry
        // first. No threshold: "soon" is policy, and this endpoint reports
        // facts.
        app.MapGet("/api/registrations/expiring", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListExpiringRegistrationsHandler handler,
        CancellationToken cancellationToken)
        => Results.Ok(await handler.HandleAsync(cancellationToken));
}
