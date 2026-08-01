using RegOS.Interaction.Application.Commands.RecordMeetingOutcome;
using RegOS.Interaction.Domain.Meetings;

namespace RegOS.Api.Endpoints.Meetings;

public static class RecordMeetingOutcomeEndpoint
{
    public static IEndpointRouteBuilder MapRecordMeetingOutcome(
        this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/meetings/{meetingId:guid}/outcome", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid meetingId,
        RecordMeetingOutcomeRequest request,
        RecordMeetingOutcomeHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RecordMeetingOutcomeCommand(
                HaMeetingId.From(meetingId), request.Minutes, request.Outcome),
            cancellationToken);

        return Results.NoContent();
    }
}
