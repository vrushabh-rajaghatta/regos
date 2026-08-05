using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Product.Application.Commands.RestateIngredient;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Presentations;

public static class RestateIngredientEndpoint
{
    public static IEndpointRouteBuilder MapRestateIngredient(
        this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/presentations/{presentationId:guid}/ingredients/{ingredientId:guid}",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid presentationId,
        Guid ingredientId,
        RestateIngredientRequest request,
        RestateIngredientHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RestateIngredientCommand(
                new PharmaceuticalProductDetailId(presentationId),
                new IngredientId(ingredientId),
                IngredientRoles.Parse(request.Role),
                request.NumeratorValue,
                request.NumeratorUnitCode,
                request.DenominatorValue,
                request.DenominatorUnitCode,
                request.ManufacturingSourceSiteId is { } site
                    ? OrganizationSiteId.From(site)
                    : null),
            cancellationToken);

        return Results.NoContent();
    }
}
