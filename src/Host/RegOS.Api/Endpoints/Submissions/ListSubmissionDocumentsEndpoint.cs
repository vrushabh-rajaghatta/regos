using RegOS.Submission.Application.Queries.ListSubmissionDocuments;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class ListSubmissionDocumentsEndpoint
{
    public static IEndpointRouteBuilder MapListSubmissionDocuments(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/submissions/{submissionId:guid}/documents",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        ListSubmissionDocumentsHandler handler,
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
