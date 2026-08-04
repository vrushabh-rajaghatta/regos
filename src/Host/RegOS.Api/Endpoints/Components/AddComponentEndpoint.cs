using RegOS.Product.Application.Commands.AddComponent;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Components;

public static class AddComponentEndpoint
{
    public static IEndpointRouteBuilder MapAddComponent(
        this IEndpointRouteBuilder app)
    {
        // Nested under the market, not under a parent component: a component is
        // a thing in a market that may happen to sit inside another, and
        // routing by parent would make the top-level case the awkward one.
        app.MapPost(
            "/api/medicinal-products/{medicinalProductId:guid}/components",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        AddComponentRequest request,
        AddComponentHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new AddComponentCommand(
                new MedicinalProductId(medicinalProductId),
                request.ParentComponentId is { } parent
                    ? new MedicinalProductComponentId(parent)
                    : null,
                request.ComponentTypeCode,
                request.Name,
                request.Description,
                request.Quantity,
                request.UnitOfPresentationCode,
                request.DoseFormCode),
            cancellationToken);

        return Results.Created(
            $"/api/components/{result.Id.Value}",
            new AddComponentResponse(result.Id.Value));
    }
}
