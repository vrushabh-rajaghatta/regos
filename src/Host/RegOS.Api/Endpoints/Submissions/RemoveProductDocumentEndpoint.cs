using RegOS.Submission.Application.Commands.RemoveProductDocument;
using RegOS.Submission.Application.Exceptions;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class RemoveProductDocumentEndpoint
{
    public static IEndpointRouteBuilder MapRemoveProductDocument(
        this IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "/submissions/{submissionId:guid}/documents/{submissionDocumentId:guid}",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        Guid submissionDocumentId,
        RemoveProductDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleAsync(
                new RemoveProductDocumentCommand(
                    new SubmissionId(submissionId),
                    new SubmissionDocumentId(submissionDocumentId)),
                cancellationToken);

            return Results.NoContent();
        }
        catch (SubmissionNotFoundException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound);
        }
        catch (InvalidOperationException ex)
        {
            // Aggregate invariant violated (submission not draft, or the
            // attachment is not part of this submission).
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
    }
}
