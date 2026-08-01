namespace RegOS.Api.Endpoints.Meetings;

/// <param name="Outcome">
/// What the authority concluded — not a list of what we now owe. Those are
/// commitments, with their own due dates and owners.
/// </param>
public sealed record RecordMeetingOutcomeRequest(string? Minutes, string? Outcome);
