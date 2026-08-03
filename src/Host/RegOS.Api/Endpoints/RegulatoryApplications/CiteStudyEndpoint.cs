using RegOS.RegulatoryApplication.Application.Commands.CiteStudy;
using RegOS.RegulatoryApplication.Application.Commands.StopCitingStudy;
using RegOS.RegulatoryApplication.Application.Queries.Applications.ListApplicationStudies;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;

namespace RegOS.Api.Endpoints.RegulatoryApplications;

/// <summary>
/// Which studies support a filing. Three routes on one resource, so they are
/// mapped together and named for their verbs (SC-004).
/// </summary>
public static class CiteStudyEndpoint
{
    public static IEndpointRouteBuilder MapApplicationStudyCitations(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/applications/{applicationId:guid}/studies", ListAsync);

        app.MapPost("/api/applications/{applicationId:guid}/studies", CiteAsync);

        app.MapDelete(
            "/api/applications/{applicationId:guid}/studies/{studyId:guid}",
            StopCitingAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(
        Guid applicationId,
        ListApplicationStudiesHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListApplicationStudiesQuery(
                new RegulatoryApplicationId(applicationId)),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> CiteAsync(
        Guid applicationId,
        CiteStudyRequest request,
        CiteStudyHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new CiteStudyCommand(
                new RegulatoryApplicationId(applicationId),
                request.ClinicalStudyId is { } clinical
                    ? ClinicalStudyId.From(clinical)
                    : null,
                request.NonClinicalStudyId is { } nonClinical
                    ? NonClinicalStudyId.From(nonClinical)
                    : null),
            cancellationToken);

        return Results.NoContent();
    }

    // The study is named in the path rather than the body: withdrawing a
    // citation identifies it by the study, and the kind adds nothing — an
    // application cites a study once or not at all.
    private static async Task<IResult> StopCitingAsync(
        Guid applicationId,
        Guid studyId,
        StopCitingStudyHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new StopCitingStudyCommand(
                new RegulatoryApplicationId(applicationId), studyId),
            cancellationToken);

        return Results.NoContent();
    }
}
