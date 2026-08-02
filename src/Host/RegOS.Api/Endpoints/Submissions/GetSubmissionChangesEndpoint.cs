using RegOS.Submission.Application.Queries.GetSubmissionChanges;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class GetSubmissionChangesEndpoint
{
    public static IEndpointRouteBuilder MapGetSubmissionChanges(
        this IEndpointRouteBuilder app)
    {
        // What this filing did to the sequence before it. A draft answers with
        // an empty change set rather than a 404 — "nothing filed yet" is a state
        // to render, not a missing resource.
        app.MapGet(
            "/api/submissions/{submissionId:guid}/changes",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        GetSubmissionChangesHandler handler,
        CancellationToken cancellationToken)
    {
        var changes = await handler.HandleAsync(
            new GetSubmissionChangesQuery(SubmissionId.From(submissionId)),
            cancellationToken);

        return changes is null ? Results.NotFound() : Results.Ok(changes);
    }
}
