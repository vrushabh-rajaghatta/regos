using RegOS.ReferenceData.Application.Queries.Regulatory.ListCorrespondenceTypes;

namespace RegOS.Api.Endpoints.ReferenceData;

public static class ListCorrespondenceTypesEndpoint
{
    public static IEndpointRouteBuilder MapListCorrespondenceTypes(
        this IEndpointRouteBuilder app)
    {
        // Under /api, unlike its grandfathered siblings on /master-data.
        // SC-001 holds for new work; those lists shrink, they do not grow.
        app.MapGet("/api/master-data/correspondence-types", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListCorrespondenceTypesHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListCorrespondenceTypesQuery(),
            cancellationToken);

        return Results.Ok(result);
    }
}
