using RegOS.Labeling.Application.Commands.CreateGlobalLabel;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.GlobalLabels;

public static class CreateGlobalLabelEndpoint
{
    public static IEndpointRouteBuilder MapCreateGlobalLabel(
        this IEndpointRouteBuilder app)
    {
        // Nested under the product, because a global label is always held for
        // one and carries no meaning apart from it. Operations on a label that
        // already exists are flat, under /api/global-labels — the same split
        // the market tier uses.
        app.MapPost(
            "/api/products/{globalProductId:guid}/global-labels",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid globalProductId,
        CreateGlobalLabelRequest request,
        CreateGlobalLabelHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new CreateGlobalLabelCommand(
                new GlobalProductId(globalProductId),
                request.Name,
                request.LabelTypeCode),
            cancellationToken);

        return Results.Created(
            $"/api/global-labels/{result.Id.Value}",
            new GlobalLabelResponse(result.Id.Value, result.DraftVersionId.Value));
    }
}
