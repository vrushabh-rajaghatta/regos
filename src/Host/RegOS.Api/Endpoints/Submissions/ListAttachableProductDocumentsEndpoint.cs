using RegOS.Submission.Application.Queries.ListAttachableProductDocuments;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class ListAttachableProductDocumentsEndpoint
{
    public static IEndpointRouteBuilder MapListAttachableProductDocuments(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/submissions/{submissionId:guid}/attachable-documents",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        ListAttachableProductDocumentsHandler handler,
        CancellationToken cancellationToken)
    {
        var documents = await handler.HandleAsync(
            new SubmissionId(submissionId),
            cancellationToken);

        return documents is null
            ? Results.NotFound()
            : Results.Ok(documents);
    }
}
