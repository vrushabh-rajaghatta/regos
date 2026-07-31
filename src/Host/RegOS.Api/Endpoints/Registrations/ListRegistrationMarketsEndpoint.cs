using RegOS.Registration.Application.Queries.ListRegistrationMarkets;

namespace RegOS.Api.Endpoints.Registrations;

public static class ListRegistrationMarketsEndpoint
{
    public static IEndpointRouteBuilder MapListRegistrationMarkets(
        this IEndpointRouteBuilder app)
    {
        // The entry point to the market view: where we hold anything at all.
        app.MapGet("/api/registrations/markets", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListRegistrationMarketsHandler handler,
        CancellationToken cancellationToken)
        => Results.Ok(await handler.HandleAsync(cancellationToken));
}
