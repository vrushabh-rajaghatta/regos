using RegOS.ProductDocument.Domain.IDs;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.Submission.Application.Commands.AttachProductDocument;
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
        var result = await handler.HandleAsync(
            new AttachProductDocumentCommand(
                new SubmissionId(submissionId),
                new ProductDocumentId(request.ProductDocumentId),
                request.TemplateSectionId is { } sectionId
                    ? new TemplateSectionId(sectionId)
                    : null),
            cancellationToken);

        return Results.Created(
            $"/submissions/{submissionId}/documents/{result.Id.Value}",
            new AttachProductDocumentResponse(result.Id.Value));
    }
}
