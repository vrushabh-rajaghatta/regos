using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Commands.CreateSubmission;
using RegOS.Submission.Application.Exceptions;

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
        try
        {
            var result = await handler.HandleAsync(
                new CreateSubmissionCommand(
                    new RegulatoryApplicationId(applicationId),
                    new SubmissionTypeId(request.SubmissionTypeId),
                    request.Title),
                cancellationToken);

            return Results.Created(
                $"/applications/{applicationId}/submissions/{result.Id.Value}",
                new CreateSubmissionResponse(result.Id.Value));
        }
        catch (ApplicationNotFoundException ex)
        {
            // The addressed resource (the Application) does not exist.
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound);
        }
        catch (BusinessRuleViolationException ex)
        {
            // A cross-aggregate business rule was violated.
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
