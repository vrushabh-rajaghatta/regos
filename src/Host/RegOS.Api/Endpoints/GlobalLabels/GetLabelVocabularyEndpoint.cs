using RegOS.ReferenceData.Application.Queries.Labels.GetLabelVocabulary;

namespace RegOS.Api.Endpoints.GlobalLabels;

public static class GetLabelVocabularyEndpoint
{
    public static IEndpointRouteBuilder MapGetLabelVocabulary(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/labels/vocabulary", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        GetLabelVocabularyHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetLabelVocabularyQuery(), cancellationToken);

        return Results.Ok(result);
    }
}
