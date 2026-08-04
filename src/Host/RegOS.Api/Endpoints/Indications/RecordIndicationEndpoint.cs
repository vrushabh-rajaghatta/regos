using RegOS.Labeling.Application.Commands.RecordIndication;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Indications;

public static class RecordIndicationEndpoint
{
    public static IEndpointRouteBuilder MapRecordIndication(
        this IEndpointRouteBuilder app)
    {
        // Nested under the market: an indication is approved for a product in a
        // jurisdiction, and carries no meaning apart from one.
        app.MapPost(
            "/api/medicinal-products/{medicinalProductId:guid}/indications",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        RecordIndicationRequest request,
        RecordIndicationHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new RecordIndicationCommand(
                new MedicinalProductId(medicinalProductId),
                request.ConditionCode,
                request.LabelText,
                request.ApprovedOn),
            cancellationToken);

        return Results.Created(
            $"/api/indications/{result.Id.Value}",
            new IndicationResponse(result.Id.Value));
    }
}
