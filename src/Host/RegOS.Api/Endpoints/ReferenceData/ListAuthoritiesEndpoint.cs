using RegOS.ReferenceData.Application.Queries.Regulatory.ListAuthorities;

namespace RegOS.Api.Endpoints.ReferenceData;

public static class ListAuthoritiesEndpoint
{
    public static IEndpointRouteBuilder MapListAuthorities(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/master-data/authorities",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListAuthoritiesHandler handler,
        CancellationToken cancellationToken)
    {
        var result =
            await handler.HandleAsync(cancellationToken);

        return Results.Ok(result);
    }
}
