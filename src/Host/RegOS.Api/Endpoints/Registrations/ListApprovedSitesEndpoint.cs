using RegOS.Product.Domain.Product;
using RegOS.Registration.Application.Queries.ListApprovedSites;

namespace RegOS.Api.Endpoints.Registrations;

public static class ListApprovedSitesEndpoint
{
    /// <remarks>
    /// <b>Nested under the market, not the licence.</b> The question is "which
    /// sites are approved <em>here</em>?", and a market has several licences —
    /// asking one of them answers something narrower than anybody has.
    /// </remarks>
    public static IEndpointRouteBuilder MapListApprovedSites(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/medicinal-products/{medicinalProductId:guid}/approved-sites",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        ListApprovedSitesHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListApprovedSitesQuery(
                MedicinalProductId.From(medicinalProductId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
