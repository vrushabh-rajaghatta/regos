using RegOS.Product.Application.Commands.AddTradeName;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.MedicinalProducts;

public static class AddTradeNameEndpoint
{
    public static IEndpointRouteBuilder MapAddTradeName(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/medicinal-products/{medicinalProductId:guid}/trade-names",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        AddTradeNameRequest request,
        AddTradeNameHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new AddTradeNameCommand(
                new MedicinalProductId(medicinalProductId),
                request.Language,
                request.Name),
            cancellationToken);

        return Results.Created(
            $"/api/medicinal-products/{medicinalProductId}/trade-names/{result.Id.Value}",
            new AddTradeNameResponse(result.Id.Value));
    }
}
