using RegOS.Labeling.Application.Commands.CreateLocalLabel;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.LocalLabels;

public static class CreateLocalLabelEndpoint
{
    public static IEndpointRouteBuilder MapCreateLocalLabel(
        this IEndpointRouteBuilder app)
    {
        // Nested under the market, because a local label is always held for one
        // and carries no meaning apart from it. Operations on a label that
        // already exists are flat, under /api/local-labels.
        app.MapPost(
            "/api/medicinal-products/{medicinalProductId:guid}/local-labels",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        CreateLocalLabelRequest request,
        CreateLocalLabelHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new CreateLocalLabelCommand(
                new MedicinalProductId(medicinalProductId),
                request.LabelTypeCode,
                request.Language),
            cancellationToken);

        return Results.Created(
            $"/api/local-labels/{result.Id.Value}",
            new LocalLabelResponse(
                result.Id.Value, result.DraftRevisionId.Value));
    }
}
