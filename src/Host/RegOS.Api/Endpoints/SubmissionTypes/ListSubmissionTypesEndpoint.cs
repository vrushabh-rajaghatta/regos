using RegOS.ReferenceData.Application.Queries.SubmissionTypes.ListSubmissionTypes;

namespace RegOS.Api.Endpoints.SubmissionTypes;

public static class ListSubmissionTypesEndpoint
{
    public static IEndpointRouteBuilder MapListSubmissionTypes(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/submission-types",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListSubmissionTypesHandler handler,
        CancellationToken cancellationToken,
        Guid? authorityId)
    {
        var result =
            await handler.HandleAsync(authorityId, cancellationToken);

        return Results.Ok(result);
    }
}
