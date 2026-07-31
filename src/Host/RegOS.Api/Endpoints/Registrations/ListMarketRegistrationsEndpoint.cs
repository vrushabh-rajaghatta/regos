using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.Registration.Application.Queries.ListMarketRegistrations;

namespace RegOS.Api.Endpoints.Registrations;

public static class ListMarketRegistrationsEndpoint
{
    public static IEndpointRouteBuilder MapListMarketRegistrations(
        this IEndpointRouteBuilder app)
    {
        // "What do we hold in this market?" — the mirror of the product
        // portfolio, scoped by the country rather than the product.
        app.MapGet("/api/countries/{countryId:guid}/registrations", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid countryId,
        ListMarketRegistrationsHandler handler,
        CancellationToken cancellationToken)
    {
        var registrations = await handler.HandleAsync(
            new CountryId(countryId),
            cancellationToken);

        return registrations is null
            ? Results.NotFound()
            : Results.Ok(registrations);
    }
}
