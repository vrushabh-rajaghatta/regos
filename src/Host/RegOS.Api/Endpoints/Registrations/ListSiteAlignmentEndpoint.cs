using RegOS.Product.Domain.Product;
using RegOS.Registration.Application.Queries.ListSiteAlignment;

namespace RegOS.Api.Endpoints.Registrations;

public static class ListSiteAlignmentEndpoint
{
    /// <remarks>
    /// <b>The route the epic exists for.</b> Nested under the market, because
    /// both halves of the comparison are market-scoped: operations hang off the
    /// market-local product, approvals off its licences.
    /// </remarks>
    public static IEndpointRouteBuilder MapListSiteAlignment(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/medicinal-products/{medicinalProductId:guid}/site-alignment",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        ListSiteAlignmentHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListSiteAlignmentQuery(
                MedicinalProductId.From(medicinalProductId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
