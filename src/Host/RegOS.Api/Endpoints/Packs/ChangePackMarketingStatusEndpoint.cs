using RegOS.Product.Application.Commands.ChangePackMarketingStatus;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Packs;

public static class ChangePackMarketingStatusEndpoint
{
    public static IEndpointRouteBuilder MapChangePackMarketingStatus(
        this IEndpointRouteBuilder app)
    {
        // POST rather than PUT: this appends a dated entry to a history, it
        // does not replace a value. The current status is a consequence.
        app.MapPost(
            "/api/packaged-products/{packagedProductId:guid}/marketing-status",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid packagedProductId,
        ChangePackMarketingStatusRequest request,
        ChangePackMarketingStatusHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ChangePackMarketingStatusCommand(
                PackagedProductId.From(packagedProductId),
                request.Status,
                request.OccurredOn,
                request.Note),
            cancellationToken);

        return Results.NoContent();
    }
}
