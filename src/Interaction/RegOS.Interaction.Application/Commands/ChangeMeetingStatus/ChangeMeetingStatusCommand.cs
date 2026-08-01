using RegOS.Interaction.Domain.Meetings;

namespace RegOS.Interaction.Application.Commands.ChangeMeetingStatus;

public sealed record ChangeMeetingStatusCommand(
    HaMeetingId MeetingId,
    HaMeetingStatus Target,
    DateOnly OccurredOn,
    string? Note);
