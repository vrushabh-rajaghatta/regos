using RegOS.Product.Application.Commands.RemoveComponent;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Components;

public static class RemoveComponentEndpoint
{
    public static IEndpointRouteBuilder MapRemoveComponent(
        this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/components/{componentId:guid}", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid componentId,
        RemoveComponentHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RemoveComponentCommand(
                new MedicinalProductComponentId(componentId)),
            cancellationToken);

        return Results.NoContent();
    }
}
