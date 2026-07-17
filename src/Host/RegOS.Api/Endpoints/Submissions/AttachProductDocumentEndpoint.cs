using RegOS.ProductDocument.Domain.IDs;
using RegOS.Submission.Application.Commands.AttachProductDocument;
using RegOS.Submission.Application.Exceptions;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class AttachProductDocumentEndpoint
{
    public static IEndpointRouteBuilder MapAttachProductDocument(
        this IEndpointRouteBuilder app)
    {
        // Managing the submission's document collection — the client posts a
        // document to include, not a SubmissionDocument entity.
        app.MapPost(
            "/submissions/{submissionId:guid}/documents",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        AttachProductDocumentRequest request,
        AttachProductDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(
                new AttachProductDocumentCommand(
                    new SubmissionId(submissionId),
                    new ProductDocumentId(request.ProductDocumentId)),
                cancellationToken);

            return Results.Created(
                $"/submissions/{submissionId}/documents/{result.Id.Value}",
                new AttachProductDocumentResponse(result.Id.Value));
        }
        catch (SubmissionNotFoundException ex)
        {
            // The addressed resource (the Submission) does not exist.
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound);
        }
        catch (BusinessRuleViolationException ex)
        {
            // Cross-aggregate validation failed (unknown/inactive document,
            // product mismatch).
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
        catch (InvalidOperationException ex)
        {
            // Aggregate invariant violated (duplicate, submission not draft).
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
    }
}
