using RegOS.ReferenceData.Application.Queries.Clinical.GetClinicalVocabulary;

namespace RegOS.Api.Endpoints.Indications;

public static class GetClinicalVocabularyEndpoint
{
    public static IEndpointRouteBuilder MapGetClinicalVocabulary(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/indications/vocabulary", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        GetClinicalVocabularyHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetClinicalVocabularyQuery(), cancellationToken);

        return Results.Ok(result);
    }
}
