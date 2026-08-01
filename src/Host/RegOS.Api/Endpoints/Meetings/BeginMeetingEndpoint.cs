using RegOS.Interaction.Application.Commands.BeginMeeting;
using RegOS.Interaction.Domain.Meetings;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Api.Endpoints.Meetings;

public static class BeginMeetingEndpoint
{
    public static IEndpointRouteBuilder MapBeginMeeting(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/meetings", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        BeginMeetingRequest request,
        BeginMeetingHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<HaMeetingStatus>(
                request.InitialStatus, ignoreCase: true, out var initial))
        {
            throw new DomainException(
                "A meeting begins either Requested (we asked) or Granted (they called it).");
        }

        var result = await handler.HandleAsync(
            new BeginMeetingCommand(
                new AuthorityId(request.AuthorityId),
                request.Subject,
                initial,
                request.OccurredOn,
                request.ScheduledFor,
                request.AuthorityDivisionId is { } division
                    ? new AuthorityDivisionId(division)
                    : null,
                request.RegulatoryApplicationId is { } application
                    ? new RegulatoryApplicationId(application)
                    : null),
            cancellationToken);

        return Results.Created(
            $"/api/meetings/{result.MeetingId.Value}",
            new BeginMeetingResponse(result.MeetingId.Value));
    }
}
