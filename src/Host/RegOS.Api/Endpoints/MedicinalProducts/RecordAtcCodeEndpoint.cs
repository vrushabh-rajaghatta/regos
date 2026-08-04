using RegOS.Product.Application.Commands.RecordAtcCode;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.MedicinalProducts;

public static class RecordAtcCodeEndpoint
{
    public static IEndpointRouteBuilder MapRecordAtcCode(
        this IEndpointRouteBuilder app)
    {
        // PUT, not PATCH: the whole of what this route addresses is one value,
        // and a blank body clears it.
        app.MapPut(
            "/api/medicinal-products/{medicinalProductId:guid}/atc-code",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        RecordAtcCodeRequest request,
        RecordAtcCodeHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RecordAtcCodeCommand(
                new MedicinalProductId(medicinalProductId), request.AtcCode),
            cancellationToken);

        return Results.NoContent();
    }
}
