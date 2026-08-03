using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;
using RegOS.Submission.Application.Commands.ReportStudyOnPlacement;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class ReportStudyOnPlacementEndpoint
{
    public static IEndpointRouteBuilder MapReportStudyOnPlacement(
        this IEndpointRouteBuilder app)
    {
        // `/study`, a sibling of `/placement`: both are facts about where the
        // document sits, and neither is a fact about the document.
        app.MapPut(
            "/api/submissions/{submissionId:guid}/documents/{documentId:guid}/study",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        Guid documentId,
        ReportStudyOnPlacementRequest request,
        ReportStudyOnPlacementHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ReportStudyOnPlacementCommand(
                new SubmissionId(submissionId),
                new SubmissionDocumentId(documentId),
                request.ClinicalStudyId is { } clinical
                    ? ClinicalStudyId.From(clinical)
                    : null,
                request.NonClinicalStudyId is { } nonClinical
                    ? NonClinicalStudyId.From(nonClinical)
                    : null,
                request.FileTag),
            cancellationToken);

        return Results.NoContent();
    }
}
