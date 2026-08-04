using RegOS.ReferenceData.Application.Queries.Presentations.GetPharmaceuticalVocabulary;

namespace RegOS.Api.Endpoints.Presentations;

public static class GetPharmaceuticalVocabularyEndpoint
{
    public static IEndpointRouteBuilder MapGetPharmaceuticalVocabulary(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/presentations/vocabulary", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        GetPharmaceuticalVocabularyHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetPharmaceuticalVocabularyQuery(), cancellationToken);

        return Results.Ok(result);
    }
}
