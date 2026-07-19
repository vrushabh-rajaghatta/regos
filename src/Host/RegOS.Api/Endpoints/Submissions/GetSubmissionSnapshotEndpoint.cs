using RegOS.Submission.Application.Queries.GetSubmissionSnapshot;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class GetSubmissionSnapshotEndpoint
{
    public static IEndpointRouteBuilder MapGetSubmissionSnapshot(
        this IEndpointRouteBuilder app)
    {
        // The API speaks the ubiquitous language: "show the published submission",
        // not "fetch snapshot #8". The snapshot is an internal implementation detail.
        app.MapGet(
            "/submissions/{submissionId:guid}/snapshot",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        GetSubmissionSnapshotHandler handler,
        CancellationToken cancellationToken)
    {
        var dossier = await handler.HandleAsync(
            new SubmissionId(submissionId),
            cancellationToken);

        // No published dossier for this submission (not published) -> 404.
        return dossier is null
            ? Results.NotFound()
            : Results.Ok(dossier);
    }
}
