using RegOS.ReferenceData.Application.Queries.DocumentTypes.ListDocumentTypes;

namespace RegOS.Api.Endpoints.ReferenceData;

public static class ListDocumentTypesEndpoint
{
    public static IEndpointRouteBuilder MapListDocumentTypes(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/reference-data/document-types",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListDocumentTypesHandler handler,
        CancellationToken cancellationToken)
    {
        var result =
            await handler.HandleAsync(cancellationToken);

        return Results.Ok(result);
    }
}
