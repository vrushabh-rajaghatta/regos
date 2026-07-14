using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Queries.ListSubmissions;

namespace RegOS.Api.Endpoints.Submissions;

public static class ListSubmissionsEndpoint
{
    public static IEndpointRouteBuilder MapListSubmissions(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/applications/{applicationId:guid}/submissions",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid applicationId,
        ListSubmissionsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new RegulatoryApplicationId(applicationId),
            cancellationToken);

        return Results.Ok(result);
    }
}
