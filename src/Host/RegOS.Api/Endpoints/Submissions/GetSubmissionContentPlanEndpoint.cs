using RegOS.Submission.Application.Queries.GetSubmissionContentPlan;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class GetSubmissionContentPlanEndpoint
{
    public static IEndpointRouteBuilder MapGetSubmissionContentPlan(
        this IEndpointRouteBuilder app)
    {
        // The dossier as the blueprint's tree of placeholders, plus what fills
        // them. A submission bound to no blueprint returns an empty structure,
        // not a 404 — that is a state to render, not a missing resource.
        app.MapGet(
            "/submissions/{submissionId:guid}/content-plan",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        GetSubmissionContentPlanHandler handler,
        CancellationToken cancellationToken)
    {
        var plan = await handler.HandleAsync(
            new SubmissionId(submissionId),
            cancellationToken);

        return plan is null ? Results.NotFound() : Results.Ok(plan);
    }
}
