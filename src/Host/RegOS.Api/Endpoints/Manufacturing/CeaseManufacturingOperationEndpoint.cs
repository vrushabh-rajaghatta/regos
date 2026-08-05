using RegOS.Product.Application.Commands.CeaseManufacturingOperation;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Manufacturing;

public static class CeaseManufacturingOperationEndpoint
{
    /// <remarks>
    /// <c>PUT</c> on a sub-resource rather than <c>DELETE</c>, and the verb is
    /// the decision: a site that made this product for four years made it, so
    /// the period is closed rather than removed (ES-018). A transfer is this
    /// call followed by a new operation.
    /// </remarks>
    public static IEndpointRouteBuilder MapCeaseManufacturingOperation(
        this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/manufacturing-operations/{manufacturingOperationId:guid}/cessation",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid manufacturingOperationId,
        CeaseManufacturingOperationRequest request,
        CeaseManufacturingOperationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new CeaseManufacturingOperationCommand(
                ManufacturingOperationId.From(manufacturingOperationId),
                request.CeasedOn),
            cancellationToken);

        return Results.NoContent();
    }
}
