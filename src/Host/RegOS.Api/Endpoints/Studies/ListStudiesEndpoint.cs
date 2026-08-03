using RegOS.Study.Application.Queries.ListStudies;

namespace RegOS.Api.Endpoints.Studies;

public static class ListStudiesEndpoint
{
    public static IEndpointRouteBuilder MapListStudies(
        this IEndpointRouteBuilder app)
    {
        // One list over two aggregates — the read composes (ADR-040 §3), which
        // is why this route is not /api/studies/clinical plus a second call.
        app.MapGet("/api/studies", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListStudiesHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListStudiesQuery(), cancellationToken);

        return Results.Ok(result);
    }
}
