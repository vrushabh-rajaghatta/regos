using RegOS.Submission.Application.Commands.RemoveSubmissionRole;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class RemoveSubmissionRoleEndpoint
{
    public static IEndpointRouteBuilder MapRemoveSubmissionRole(
        this IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "/api/submissions/{submissionId:guid}/roles/{roleId:guid}",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        Guid roleId,
        RemoveSubmissionRoleHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RemoveSubmissionRoleCommand(
                SubmissionId.From(submissionId),
                SubmissionRoleId.From(roleId)),
            cancellationToken);

        return Results.NoContent();
    }
}
