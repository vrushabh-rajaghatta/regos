using RegOS.Labeling.Application.Queries.ListIndications;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Indications;

public static class ListIndicationsEndpoint
{
    public static IEndpointRouteBuilder MapListIndications(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/medicinal-products/{medicinalProductId:guid}/indications",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        ListIndicationsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListIndicationsQuery(new MedicinalProductId(medicinalProductId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
