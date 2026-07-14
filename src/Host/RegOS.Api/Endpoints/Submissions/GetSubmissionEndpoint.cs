using RegOS.Submission.Application.Queries.GetSubmission;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class GetSubmissionEndpoint
{
    public static IEndpointRouteBuilder MapGetSubmission(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/submissions/{submissionId:guid}",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        GetSubmissionHandler handler,
        CancellationToken cancellationToken)
    {
        var submission = await handler.HandleAsync(
            new SubmissionId(submissionId),
            cancellationToken);

        return submission is null
            ? Results.NotFound()
            : Results.Ok(submission);
    }
}
