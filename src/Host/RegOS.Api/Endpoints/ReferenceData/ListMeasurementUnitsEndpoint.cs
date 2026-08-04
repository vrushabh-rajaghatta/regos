using RegOS.ReferenceData.Application.Queries.Measurement.ListMeasurementUnits;

namespace RegOS.Api.Endpoints.ReferenceData;

public static class ListMeasurementUnitsEndpoint
{
    public static IEndpointRouteBuilder MapListMeasurementUnits(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/measurement-units", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListMeasurementUnitsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListMeasurementUnitsQuery(), cancellationToken);

        return Results.Ok(result);
    }
}
