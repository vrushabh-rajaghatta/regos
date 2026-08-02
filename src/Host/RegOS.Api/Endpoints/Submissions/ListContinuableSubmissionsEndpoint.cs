using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Queries.ListContinuableSubmissions;

namespace RegOS.Api.Endpoints.Submissions;

public static class ListContinuableSubmissionsEndpoint
{
    public static IEndpointRouteBuilder MapListContinuableSubmissions(
        this IEndpointRouteBuilder app)
    {
        // Starts /api, unlike its siblings in this folder: SC-001 is enforced,
        // and the older routes sit on a shrink-only exemption that new code
        // must not join.
        app.MapGet(
            "/api/applications/{applicationId:guid}/submissions/continuable",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid applicationId,
        ListContinuableSubmissionsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListContinuableSubmissionsQuery(
                new RegulatoryApplicationId(applicationId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
