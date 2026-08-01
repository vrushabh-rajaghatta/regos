using RegOS.Interaction.Application.Commands.ChangeMeetingStatus;
using RegOS.Interaction.Domain.Meetings;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Api.Endpoints.Meetings;

public static class ChangeMeetingStatusEndpoint
{
    public static IEndpointRouteBuilder MapChangeMeetingStatus(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/meetings/{meetingId:guid}/status", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid meetingId,
        ChangeMeetingStatusRequest request,
        ChangeMeetingStatusHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<HaMeetingStatus>(
                request.Status, ignoreCase: true, out var target))
        {
            throw new DomainException(
                "Status must be one of Granted, Declined, Held or Cancelled.");
        }

        await handler.HandleAsync(
            new ChangeMeetingStatusCommand(
                HaMeetingId.From(meetingId), target, request.OccurredOn, request.Note),
            cancellationToken);

        return Results.NoContent();
    }
}
