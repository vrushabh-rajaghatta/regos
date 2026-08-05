using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Product.Application.Commands.RecordManufacturingOperation;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Manufacturing;

public static class RecordManufacturingOperationEndpoint
{
    public static IEndpointRouteBuilder MapRecordManufacturingOperation(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/medicinal-products/{medicinalProductId:guid}/manufacturing",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        RecordManufacturingOperationRequest request,
        RecordManufacturingOperationHandler handler,
        CancellationToken cancellationToken)
    {
        var id = await handler.HandleAsync(
            new RecordManufacturingOperationCommand(
                MedicinalProductId.From(medicinalProductId),
                OrganizationSiteId.From(request.OrganizationSiteId),
                request.OperationCode,
                request.EffectiveFrom),
            cancellationToken);

        return Results.Ok(new ManufacturingOperationResponse(id.Value));
    }
}
