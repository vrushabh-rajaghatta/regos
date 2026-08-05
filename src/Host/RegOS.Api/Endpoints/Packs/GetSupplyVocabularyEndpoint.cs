using RegOS.ReferenceData.Application.Queries.Supply.GetSupplyVocabulary;

namespace RegOS.Api.Endpoints.Packs;

public static class GetSupplyVocabularyEndpoint
{
    public static IEndpointRouteBuilder MapGetSupplyVocabulary(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/packaged-products/supply-vocabulary", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        GetSupplyVocabularyHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetSupplyVocabularyQuery(), cancellationToken);

        return Results.Ok(result);
    }
}
