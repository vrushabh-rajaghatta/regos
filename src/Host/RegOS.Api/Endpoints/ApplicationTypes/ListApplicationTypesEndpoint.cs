using RegOS.ReferenceData.Application.Queries.ApplicationTypes.ListApplicationTypes;

namespace RegOS.Api.Endpoints.ApplicationTypes;

public static class ListApplicationTypesEndpoint
{
    public static IEndpointRouteBuilder MapListApplicationTypes(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/reference-data/application-types",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListApplicationTypesHandler handler,
        CancellationToken cancellationToken,
        Guid? authorityId)
    {
        var result =
            await handler.HandleAsync(
                new ListApplicationTypesQuery(authorityId), cancellationToken);

        return Results.Ok(result);
    }
}
