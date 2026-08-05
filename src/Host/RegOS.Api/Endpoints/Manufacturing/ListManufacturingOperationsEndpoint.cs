using RegOS.Product.Application.Queries.ListManufacturingOperations;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Manufacturing;

public static class ListManufacturingOperationsEndpoint
{
    /// <remarks>
    /// <b>Nested under the market, not the site.</b> The question is <em>"which
    /// sites make <b>this</b> product?"</em>, and the market is what the answer
    /// is compared against — a licence approves sites for one market at a time.
    /// </remarks>
    public static IEndpointRouteBuilder MapListManufacturingOperations(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/medicinal-products/{medicinalProductId:guid}/manufacturing",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        ListManufacturingOperationsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListManufacturingOperationsQuery(
                MedicinalProductId.From(medicinalProductId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
