using RegOS.Interaction.Domain.Meetings;

namespace RegOS.Interaction.Application.Commands.RecordMeetingOutcome;

public sealed record RecordMeetingOutcomeCommand(
    HaMeetingId MeetingId,
    string? Minutes,
    string? Outcome);
