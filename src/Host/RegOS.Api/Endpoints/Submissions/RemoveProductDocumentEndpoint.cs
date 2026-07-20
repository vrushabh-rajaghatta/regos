using RegOS.Submission.Application.Commands.RemoveProductDocument;
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
        await handler.HandleAsync(
            new RemoveProductDocumentCommand(
                new SubmissionId(submissionId),
                new SubmissionDocumentId(submissionDocumentId)),
            cancellationToken);

        return Results.NoContent();
    }
}
