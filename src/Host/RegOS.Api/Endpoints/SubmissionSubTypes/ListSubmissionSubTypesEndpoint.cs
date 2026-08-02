using RegOS.ReferenceData.Application.Queries.SubmissionSubTypes.ListSubmissionSubTypes;

namespace RegOS.Api.Endpoints.SubmissionSubTypes;

public static class ListSubmissionSubTypesEndpoint
{
    public static IEndpointRouteBuilder MapListSubmissionSubTypes(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/reference-data/submission-sub-types",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListSubmissionSubTypesHandler handler,
        CancellationToken cancellationToken,
        Guid? authorityId)
    {
        var result =
            await handler.HandleAsync(
                new ListSubmissionSubTypesQuery(authorityId), cancellationToken);

        return Results.Ok(result);
    }
}
