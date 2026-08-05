using RegOS.Labeling.Application.Queries.GetLabelLanguageCoverage;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.LocalLabels;

public static class GetLabelLanguageCoverageEndpoint
{
    /// <remarks>
    /// Its own read rather than a field on the label list: the expected half
    /// comes from the country and the recorded half from the tenant, and a
    /// caller that wanted only the labels should not have to pay for geography.
    /// </remarks>
    public static IEndpointRouteBuilder MapGetLabelLanguageCoverage(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/medicinal-products/{medicinalProductId:guid}/label-languages",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        GetLabelLanguageCoverageHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetLabelLanguageCoverageQuery(
                MedicinalProductId.From(medicinalProductId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
