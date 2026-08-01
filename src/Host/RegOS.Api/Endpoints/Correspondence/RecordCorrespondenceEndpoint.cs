using RegOS.Interaction.Application.Commands.RecordCorrespondence;
using RegOS.Interaction.Domain.Correspondence;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.Regulatory.Correspondence;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Api.Endpoints.Correspondence;

public static class RecordCorrespondenceEndpoint
{
    public static IEndpointRouteBuilder MapRecordCorrespondence(
        this IEndpointRouteBuilder app)
    {
        // Not nested under an application: correspondence may concern one, a
        // submission, a registration or nothing at all, and a route that named
        // one of them would make the other three second-class.
        app.MapPost("/api/correspondence", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        RecordCorrespondenceRequest request,
        RecordCorrespondenceHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CorrespondenceDirection>(
                request.Direction, ignoreCase: true, out var direction))
        {
            throw new DomainException(
                "Direction must be either Inbound or Outbound.");
        }

        var result = await handler.HandleAsync(
            new RecordCorrespondenceCommand(
                new AuthorityId(request.AuthorityId),
                new CorrespondenceTypeId(request.CorrespondenceTypeId),
                direction,
                request.Subject,
                request.OccurredOn,
                request.ResponseDueOn,
                request.AuthorityReference,
                request.RegulatoryApplicationId is { } applicationId
                    ? new RegulatoryApplicationId(applicationId)
                    : null,
                request.SubmissionId is { } submissionId
                    ? new SubmissionId(submissionId)
                    : null,
                request.RegistrationId is { } registrationId
                    ? new RegistrationId(registrationId)
                    : null),
            cancellationToken);

        return Results.Created(
            $"/api/correspondence/{result.CorrespondenceId.Value}",
            new RecordCorrespondenceResponse(result.CorrespondenceId.Value));
    }
}
