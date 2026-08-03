using RegOS.RegulatoryApplication.Application.Commands.RecordApplicationNumber;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Api.Endpoints.RegulatoryApplications;

public static class RecordApplicationNumberEndpoint
{
    public static IEndpointRouteBuilder MapRecordApplicationNumber(
        this IEndpointRouteBuilder app)
    {
        // PUT: the number an authority assigned is a single value with one
        // correct answer, and recording it twice is the same request twice.
        app.MapPut(
            "/api/applications/{applicationId:guid}/application-number",
            HandleAsync)
            .WithName("RecordApplicationNumber")
            .WithSummary("Record the number the authority assigned")
            .WithTags("Regulatory Applications");

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid applicationId,
        RecordApplicationNumberRequest request,
        RecordApplicationNumberHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RecordApplicationNumberCommand(
                new RegulatoryApplicationId(applicationId),
                request.ApplicationNumber),
            cancellationToken);

        return Results.NoContent();
    }
}

/// <param name="ApplicationNumber">
/// As the authority issued it. No format is imposed here — FDA's six digits are
/// FDA's, and the check lives at the FDA boundary (ADR-055).
/// </param>
public sealed record RecordApplicationNumberRequest(string ApplicationNumber);
