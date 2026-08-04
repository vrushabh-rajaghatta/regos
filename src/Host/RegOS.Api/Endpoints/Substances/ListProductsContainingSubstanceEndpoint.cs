using RegOS.Product.Application.Queries.ListProductsContainingSubstance;
using RegOS.ReferenceData.Domain.Substances;

namespace RegOS.Api.Endpoints.Substances;

public static class ListProductsContainingSubstanceEndpoint
{
    public static IEndpointRouteBuilder MapListProductsContainingSubstance(
        this IEndpointRouteBuilder app)
    {
        // Routed from the substance, because that is where the question is
        // asked from — even though the handler lives in Product, which is the
        // only context that can see both ends without inverting a dependency.
        // Where a route sits and where its code lives are different decisions.
        app.MapGet("/api/substances/{substanceId:guid}/products", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid substanceId,
        ListProductsContainingSubstanceHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListProductsContainingSubstanceQuery(new SubstanceId(substanceId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
