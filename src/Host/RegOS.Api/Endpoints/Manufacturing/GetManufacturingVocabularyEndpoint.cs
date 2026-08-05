using RegOS.ReferenceData.Application.Queries.Manufacturing.GetManufacturingVocabulary;

namespace RegOS.Api.Endpoints.Manufacturing;

public static class GetManufacturingVocabularyEndpoint
{
    public static IEndpointRouteBuilder MapGetManufacturingVocabulary(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/manufacturing-operations/vocabulary", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        GetManufacturingVocabularyHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetManufacturingVocabularyQuery(), cancellationToken);

        return Results.Ok(result);
    }
}
