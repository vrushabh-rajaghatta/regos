using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.Submission.Application.Commands.AssignSubmissionRole;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class AssignSubmissionRoleEndpoint
{
    public static IEndpointRouteBuilder MapAssignSubmissionRole(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/submissions/{submissionId:guid}/roles", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid submissionId,
        AssignSubmissionRoleRequest request,
        AssignSubmissionRoleHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new AssignSubmissionRoleCommand(
                SubmissionId.From(submissionId),
                ContactId.From(request.ContactId),
                new ContactRoleId(request.RoleId)),
            cancellationToken);

        return Results.Created(
            $"/api/submissions/{submissionId}/roles/{result.Id.Value}",
            new AssignSubmissionRoleResponse(result.Id.Value));
    }
}
