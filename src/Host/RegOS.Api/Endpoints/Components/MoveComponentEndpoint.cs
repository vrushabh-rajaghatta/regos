using RegOS.Product.Application.Commands.MoveComponent;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Components;

public static class MoveComponentEndpoint
{
    public static IEndpointRouteBuilder MapMoveComponent(
        this IEndpointRouteBuilder app)
    {
        // Its own route because it is its own decision. A cycle and an
        // over-deep tree are both refused here and nowhere else, and a caller
        // reading the API should be able to see that changing position is not
        // the same kind of act as changing a description.
        app.MapPut("/api/components/{componentId:guid}/parent", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid componentId,
        MoveComponentRequest request,
        MoveComponentHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new MoveComponentCommand(
                new MedicinalProductComponentId(componentId),
                request.NewParentComponentId is { } parent
                    ? new MedicinalProductComponentId(parent)
                    : null),
            cancellationToken);

        return Results.NoContent();
    }
}
