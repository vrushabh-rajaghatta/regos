using RegOS.Submission.Application.Queries.ListStudyFilings;

namespace RegOS.Api.Endpoints.Studies;

public static class ListStudyFilingsEndpoint
{
    public static IEndpointRouteBuilder MapListStudyFilings(
        this IEndpointRouteBuilder app)
    {
        // Addressed by the study, because that is what the caller has — and by
        // a plain guid, because the answer does not depend on which of the two
        // aggregates it came from.
        app.MapGet("/api/studies/{studyId:guid}/filings", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid studyId,
        ListStudyFilingsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListStudyFilingsQuery(studyId), cancellationToken);

        return Results.Ok(result);
    }
}
