using RegOS.Interaction.Application.Queries.ListCorrespondence;
using RegOS.Interaction.Domain.Correspondence;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.Regulatory.Correspondence;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Domain.Submission;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Api.Endpoints.Correspondence;

public static class ListCorrespondenceEndpoint
{
    public static IEndpointRouteBuilder MapListCorrespondence(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/correspondence", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListCorrespondenceHandler handler,
        CancellationToken cancellationToken,
        Guid? authorityId = null,
        Guid? correspondenceTypeId = null,
        string? direction = null,
        Guid? regulatoryApplicationId = null,
        Guid? submissionId = null)
    {
        CorrespondenceDirection? parsedDirection = null;

        if (!string.IsNullOrWhiteSpace(direction))
        {
            if (!Enum.TryParse<CorrespondenceDirection>(
                    direction, ignoreCase: true, out var value))
            {
                throw new DomainException(
                    "Direction must be either Inbound or Outbound.");
            }

            parsedDirection = value;
        }

        var result = await handler.HandleAsync(
            new ListCorrespondenceQuery(
                authorityId is { } authority ? new AuthorityId(authority) : null,
                correspondenceTypeId is { } typeId
                    ? new CorrespondenceTypeId(typeId)
                    : null,
                parsedDirection,
                regulatoryApplicationId is { } applicationId
                    ? new RegulatoryApplicationId(applicationId)
                    : null,
                submissionId is { } submission
                    ? SubmissionId.From(submission)
                    : null),
            cancellationToken);

        return Results.Ok(result);
    }
}
