using RegOS.Submission.Application.Queries.ListSubmissionRoles;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class ListSubmissionRolesEndpoint
{
    public static IEndpointRouteBuilder MapListSubmissionRoles(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/submissions/{submissionId:guid}/roles", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        ListSubmissionRolesHandler handler,
        CancellationToken cancellationToken)
    {
        var roles = await handler.HandleAsync(
            new ListSubmissionRolesQuery(SubmissionId.From(submissionId)),
            cancellationToken);

        return roles is null ? Results.NotFound() : Results.Ok(roles);
    }
}
