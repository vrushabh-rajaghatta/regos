using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.Submission.Application.Commands.PlaceSubmissionDocument;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class PlaceSubmissionDocumentEndpoint
{
    public static IEndpointRouteBuilder MapPlaceSubmissionDocument(
        this IEndpointRouteBuilder app)
    {
        // PUT, not PATCH: the body states the whole placement, so sending it
        // twice lands in the same place. A null section clears the placement —
        // which is what dragging a document out of the tree will call.
        app.MapPut(
            "/submissions/{submissionId:guid}/documents/{documentId:guid}/placement",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        Guid documentId,
        PlaceSubmissionDocumentRequest request,
        PlaceSubmissionDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new PlaceSubmissionDocumentCommand(
                new SubmissionId(submissionId),
                new SubmissionDocumentId(documentId),
                request.TemplateSectionId is { } sectionId
                    ? new TemplateSectionId(sectionId)
                    : null),
            cancellationToken);

        return Results.NoContent();
    }
}
