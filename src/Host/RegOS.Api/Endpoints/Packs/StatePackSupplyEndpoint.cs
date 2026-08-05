using RegOS.Product.Application.Commands.StatePackSupply;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Packs;

public static class StatePackSupplyEndpoint
{
    /// <remarks>
    /// Its own route rather than part of the pack itself: restating what a pack
    /// <em>is</em> and stating how it may be <em>supplied</em> are two acts, and
    /// the second arrives long after the first — usually when stability data
    /// does.
    /// </remarks>
    public static IEndpointRouteBuilder MapStatePackSupply(
        this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/packaged-products/{packagedProductId:guid}/supply", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid packagedProductId,
        StatePackSupplyRequest request,
        StatePackSupplyHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new StatePackSupplyCommand(
                PackagedProductId.From(packagedProductId),
                request.LegalStatusOfSupplyCode,
                request.ShelfLifeValue,
                request.ShelfLifeUnitCode,
                request.ShelfLifeText,
                request.StorageConditionCodes ?? [],
                request.TestedAtCodes ?? []),
            cancellationToken);

        return Results.NoContent();
    }
}
