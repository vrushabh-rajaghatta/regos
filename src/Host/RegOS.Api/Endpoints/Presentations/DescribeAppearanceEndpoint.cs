using RegOS.Product.Application.Commands.DescribeAppearance;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Presentations;

public static class DescribeAppearanceEndpoint
{
    /// <remarks>
    /// Its own route: a presentation is recorded when its dose form is known and
    /// described when somebody has seen it, which is routinely later.
    /// </remarks>
    public static IEndpointRouteBuilder MapDescribeAppearance(
        this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/presentations/{presentationId:guid}/appearance", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid presentationId,
        DescribeAppearanceRequest request,
        DescribeAppearanceHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new DescribeAppearanceCommand(
                PharmaceuticalProductDetailId.From(presentationId),
                request.ColourCodes ?? [],
                request.ShapeCode,
                request.Imprint,
                request.Description),
            cancellationToken);

        return Results.NoContent();
    }
}
