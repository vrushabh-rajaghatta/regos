using RegOS.Labeling.Application.Commands.RecordIndicationDecision;
using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Api.Endpoints.Indications;

public static class RecordIndicationDecisionEndpoint
{
    public static IEndpointRouteBuilder MapRecordIndicationDecision(
        this IEndpointRouteBuilder app)
    {
        // POST, appending to a history that is never rewritten: a regulatory
        // decision should not disappear.
        app.MapPost("/api/indications/{indicationId:guid}/decisions", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid indicationId,
        RecordDecisionRequest request,
        RecordIndicationDecisionHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RecordIndicationDecisionCommand(
                IndicationId.From(indicationId),
                request.Status,
                request.OccurredOn,
                request.Note),
            cancellationToken);

        return Results.NoContent();
    }
}
