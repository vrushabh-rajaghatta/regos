using RegOS.Interaction.Domain.Meetings;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Interaction.Application.Commands.BeginMeeting;

/// <param name="InitialStatus">
/// <c>Requested</c> when we asked, <c>Granted</c> when the authority called it.
/// A parameter rather than a constant, so the history never records a request
/// that did not happen.
/// </param>
public sealed record BeginMeetingCommand(
    AuthorityId AuthorityId,
    string Subject,
    HaMeetingStatus InitialStatus,
    DateOnly OccurredOn,
    DateOnly? ScheduledFor,
    AuthorityDivisionId? AuthorityDivisionId,
    RegulatoryApplicationId? RegulatoryApplicationId);
