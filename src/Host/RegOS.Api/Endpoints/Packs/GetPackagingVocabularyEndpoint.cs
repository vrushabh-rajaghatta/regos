using RegOS.ReferenceData.Application.Queries.Packaging.GetPackagingVocabulary;

namespace RegOS.Api.Endpoints.Packs;

public static class GetPackagingVocabularyEndpoint
{
    public static IEndpointRouteBuilder MapGetPackagingVocabulary(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/packaged-products/vocabulary", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        GetPackagingVocabularyHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetPackagingVocabularyQuery(), cancellationToken);

        return Results.Ok(result);
    }
}
