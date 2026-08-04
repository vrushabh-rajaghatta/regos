using RegOS.Product.Application.Queries.ListPacks;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Packs;

public static class ListPacksEndpoint
{
    public static IEndpointRouteBuilder MapListPacks(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/medicinal-products/{medicinalProductId:guid}/packaged-products",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        ListPacksHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListPacksQuery(new MedicinalProductId(medicinalProductId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
