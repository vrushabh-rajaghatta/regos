using RegOS.ReferenceData.Application.Queries.Substances.GetSubstanceVocabulary;

namespace RegOS.Api.Endpoints.Substances;

public static class GetSubstanceVocabularyEndpoint
{
    public static IEndpointRouteBuilder MapGetSubstanceVocabulary(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/substances/vocabulary", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        GetSubstanceVocabularyHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetSubstanceVocabularyQuery(), cancellationToken);

        return Results.Ok(result);
    }
}
