using RegOS.Submission.Application.Queries.ValidateSubmission;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class ValidateSubmissionEndpoint
{
    public static IEndpointRouteBuilder MapValidateSubmission(
        this IEndpointRouteBuilder app)
    {
        // Readiness is a query about an existing submission: the result may carry
        // zero or many issues, but "not ready" is a 200 with issues, not an error.
        app.MapGet(
            "/submissions/{submissionId:guid}/validation",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        ValidateSubmissionHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(
            new SubmissionId(submissionId),
            cancellationToken);

        return Results.Ok(response);
    }
}
