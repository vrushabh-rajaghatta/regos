using RegOS.Product.Application.Commands.RemoveTradeName;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.MedicinalProducts;

public static class RemoveTradeNameEndpoint
{
    public static IEndpointRouteBuilder MapRemoveTradeName(
        this IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "/api/medicinal-products/{medicinalProductId:guid}"
            + "/trade-names/{tradeNameId:guid}",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        Guid tradeNameId,
        RemoveTradeNameHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RemoveTradeNameCommand(
                new MedicinalProductId(medicinalProductId),
                new TradeNameId(tradeNameId)),
            cancellationToken);

        return Results.NoContent();
    }
}
