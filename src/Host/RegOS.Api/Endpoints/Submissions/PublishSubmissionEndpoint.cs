using RegOS.Submission.Application.Commands.PublishSubmission;
using RegOS.Submission.Application.Exceptions;
using RegOS.Submission.Application.Queries.ValidateSubmission;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class PublishSubmissionEndpoint
{
    public static IEndpointRouteBuilder MapPublishSubmission(
        this IEndpointRouteBuilder app)
    {
        // Publishing is a business action on the submission — finalizing it.
        app.MapPost(
            "/submissions/{submissionId:guid}/publish",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        PublishSubmissionHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(
                new PublishSubmissionCommand(new SubmissionId(submissionId)),
                cancellationToken);

            if (result.Published)
            {
                return Results.Ok();
            }

            // Not ready — return the reasons as structured data, not an error string.
            return Results.BadRequest(
                ValidateSubmissionResponse.From(result.Validation!));
        }
        catch (SubmissionNotFoundException ex)
        {
            // The addressed resource (the Submission) does not exist.
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound);
        }
    }
}
