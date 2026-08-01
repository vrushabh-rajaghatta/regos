using RegOS.Product.Application.Queries.GetMedicinalProduct;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.MedicinalProducts;

public static class GetMedicinalProductEndpoint
{
    public static IEndpointRouteBuilder MapGetMedicinalProduct(
        this IEndpointRouteBuilder app)
    {
        // Addressed by its own id, not nested under the product: a market has
        // an identity of its own, and one canonical URL whichever direction a
        // caller arrives from.
        app.MapGet(
            "/api/medicinal-products/{medicinalProductId:guid}",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        GetMedicinalProductHandler handler,
        CancellationToken cancellationToken)
    {
        var market = await handler.HandleAsync(
            new GetMedicinalProductQuery(
                new MedicinalProductId(medicinalProductId)),
            cancellationToken);

        return market is null ? Results.NotFound() : Results.Ok(market);
    }
}
