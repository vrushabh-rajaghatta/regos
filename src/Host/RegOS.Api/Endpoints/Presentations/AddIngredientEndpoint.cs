using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Product.Application.Commands.AddIngredient;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Substances;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Api.Endpoints.Presentations;

public static class AddIngredientEndpoint
{
    public static IEndpointRouteBuilder MapAddIngredient(
        this IEndpointRouteBuilder app)
    {
        // Nested under the presentation, because an ingredient is a child of
        // one and carries no meaning apart from it.
        app.MapPost(
            "/api/presentations/{presentationId:guid}/ingredients", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid presentationId,
        AddIngredientRequest request,
        AddIngredientHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new AddIngredientCommand(
                new PharmaceuticalProductDetailId(presentationId),
                new SubstanceId(request.SubstanceId),
                IngredientRoles.Parse(request.Role),
                request.NumeratorValue,
                request.NumeratorUnitCode,
                request.DenominatorValue,
                request.DenominatorUnitCode,
                request.ManufacturingSourceSiteId is { } site
                    ? OrganizationSiteId.From(site)
                    : null),
            cancellationToken);

        return Results.Created(
            $"/api/presentations/{presentationId}/ingredients/{result.Id.Value}",
            new AddIngredientResponse(result.Id.Value));
    }
}
