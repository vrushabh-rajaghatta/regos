using RegOS.Product.Application.Commands.ChangeMarketStatus;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.MedicinalProducts;

public static class ChangeMarketStatusEndpoint
{
    public static IEndpointRouteBuilder MapChangeMarketStatus(
        this IEndpointRouteBuilder app)
    {
        // POST rather than PUT: this appends a dated entry to a history, it
        // does not replace a value. The current status is a consequence.
        app.MapPost(
            "/api/medicinal-products/{medicinalProductId:guid}/market-status",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        ChangeMarketStatusRequest request,
        ChangeMarketStatusHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ChangeMarketStatusCommand(
                new MedicinalProductId(medicinalProductId),
                request.Status,
                request.OccurredOn,
                request.Note),
            cancellationToken);

        return Results.NoContent();
    }
}
