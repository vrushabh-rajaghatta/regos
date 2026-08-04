using RegOS.Product.Application.Commands.RestatePresentation;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Presentations;

public static class RestatePresentationEndpoint
{
    public static IEndpointRouteBuilder MapRestatePresentation(
        this IEndpointRouteBuilder app)
    {
        // Addressed by its own id, not through the market: a presentation is
        // its own aggregate, and routing the correction through the market
        // would imply the market is what changed.
        app.MapPut("/api/presentations/{presentationId:guid}", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid presentationId,
        PresentationRequest request,
        RestatePresentationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RestatePresentationCommand(
                new PharmaceuticalProductDetailId(presentationId),
                request.Name,
                request.Description,
                request.DoseFormCode,
                request.UnitOfPresentationCode,
                request.RouteCodes ?? []),
            cancellationToken);

        return Results.NoContent();
    }
}
