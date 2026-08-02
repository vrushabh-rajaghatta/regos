using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Application.Commands.CreateSubmission;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Submissions;

public static class CreateSubmissionEndpoint
{
    public static IEndpointRouteBuilder MapCreateSubmission(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/applications/{applicationId:guid}/submissions",
            HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid applicationId,
        CreateSubmissionRequest request,
        CreateSubmissionHandler handler,
        CancellationToken cancellationToken)
    {
        var format = SubmissionFormat.Ectd;

        if (request.Format is { } supplied
            && !Enum.TryParse(supplied, ignoreCase: true, out format))
        {
            throw new DomainException(
                "Format must be one of Ectd, Nees or Paper.");
        }

        var result = await handler.HandleAsync(
            new CreateSubmissionCommand(
                new RegulatoryApplicationId(applicationId),
                new SubmissionTypeId(request.SubmissionTypeId),
                request.Title,
                format),
            cancellationToken);

        return Results.Created(
            $"/applications/{applicationId}/submissions/{result.Id.Value}",
            new CreateSubmissionResponse(result.Id.Value));
    }
}
