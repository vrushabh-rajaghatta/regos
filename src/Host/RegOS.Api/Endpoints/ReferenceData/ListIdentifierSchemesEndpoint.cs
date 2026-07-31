using RegOS.ReferenceData.Application.Queries.Organization.ListIdentifierSchemes;

namespace RegOS.Api.Endpoints.ReferenceData;

public static class ListIdentifierSchemesEndpoint
{
    public static IEndpointRouteBuilder MapListIdentifierSchemes(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/reference-data/identifier-schemes",
            HandleAsync)
        .WithName("ListIdentifierSchemes")
        .WithSummary("List the registries that issue organization identifiers")
        .WithTags("Reference Data");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListIdentifierSchemesHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListIdentifierSchemesQuery(),
            cancellationToken);

        return Results.Ok(result);
    }
}
