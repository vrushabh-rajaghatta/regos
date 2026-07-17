using RegOS.ReferenceData.Application.Queries.Geography.ListCountries;

namespace RegOS.Api.Endpoints.ReferenceData;

public static class ListCountriesEndpoint
{
    public static IEndpointRouteBuilder MapListCountries(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/master-data/countries",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListCountriesHandler handler,
        CancellationToken cancellationToken)
    {
        var result =
            await handler.HandleAsync(cancellationToken);

        return Results.Ok(result);
    }
}
