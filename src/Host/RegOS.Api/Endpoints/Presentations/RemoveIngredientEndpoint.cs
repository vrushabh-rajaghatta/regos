using RegOS.Product.Application.Commands.RemoveIngredient;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Presentations;

public static class RemoveIngredientEndpoint
{
    public static IEndpointRouteBuilder MapRemoveIngredient(
        this IEndpointRouteBuilder app)
    {
        // A genuine delete, not a lifecycle change. ES-018 retains regulatory
        // records; a line in a formulation being drafted is not one, and a
        // composition carrying every mistake anyone made would be unreadable.
        app.MapDelete(
            "/api/presentations/{presentationId:guid}/ingredients/{ingredientId:guid}",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid presentationId,
        Guid ingredientId,
        RemoveIngredientHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RemoveIngredientCommand(
                new PharmaceuticalProductDetailId(presentationId),
                new IngredientId(ingredientId)),
            cancellationToken);

        return Results.NoContent();
    }
}
