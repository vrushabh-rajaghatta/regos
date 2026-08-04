using RegOS.Product.Application.Commands.RestateComponent;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Components;

public static class RestateComponentEndpoint
{
    public static IEndpointRouteBuilder MapRestateComponent(
        this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/components/{componentId:guid}", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid componentId,
        RestateComponentRequest request,
        RestateComponentHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RestateComponentCommand(
                new MedicinalProductComponentId(componentId),
                request.ComponentTypeCode,
                request.Name,
                request.Description,
                request.Quantity,
                request.UnitOfPresentationCode,
                request.DoseFormCode),
            cancellationToken);

        return Results.NoContent();
    }
}
