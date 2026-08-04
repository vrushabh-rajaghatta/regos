using RegOS.Product.Application.Commands.AddPresentation;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Presentations;

public static class AddPresentationEndpoint
{
    public static IEndpointRouteBuilder MapAddPresentation(
        this IEndpointRouteBuilder app)
    {
        // Nested under the market, because a presentation is always created
        // for one and carries no meaning apart from it.
        app.MapPost(
            "/api/medicinal-products/{medicinalProductId:guid}/presentations",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicinalProductId,
        PresentationRequest request,
        AddPresentationHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new AddPresentationCommand(
                new MedicinalProductId(medicinalProductId),
                request.Name,
                request.Description,
                request.DoseFormCode,
                request.UnitOfPresentationCode,
                request.RouteCodes ?? []),
            cancellationToken);

        return Results.Created(
            $"/api/presentations/{result.Id.Value}",
            new PresentationResponse(result.Id.Value));
    }
}
