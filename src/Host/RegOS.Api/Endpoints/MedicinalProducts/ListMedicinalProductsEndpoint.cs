using RegOS.Product.Application.Queries.ListMedicinalProducts;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.MedicinalProducts;

public static class ListMedicinalProductsEndpoint
{
    public static IEndpointRouteBuilder MapListMedicinalProducts(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/products/{globalProductId:guid}/medicinal-products",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalProductId,
        ListMedicinalProductsHandler handler,
        CancellationToken cancellationToken)
    {
        var markets = await handler.HandleAsync(
            new ListMedicinalProductsQuery(new GlobalProductId(globalProductId)),
            cancellationToken);

        // Null means the product itself is missing — a 404. An empty list means
        // the product exists and is in no market yet, which is ordinary.
        return markets is null
            ? Results.NotFound()
            : Results.Ok(markets);
    }
}
