using RegOS.Product.Domain.Product;
using RegOS.Registration.Application.Queries.ListAuthorisedPacks;

namespace RegOS.Api.Endpoints.Registrations;

public static class ListAuthorisedPacksEndpoint
{
    /// <remarks>
    /// <b>Nested under the market, not the licence.</b> The question is "which
    /// packs are authorised <em>here</em>?", and a market has several licences —
    /// asking one of them answers something narrower than anybody has.
    /// </remarks>
    public static IEndpointRouteBuilder MapListAuthorisedPacks(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/medicinal-products/{medicinalProductId:guid}/authorised-packs",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        ListAuthorisedPacksHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListAuthorisedPacksQuery(
                MedicinalProductId.From(medicinalProductId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
