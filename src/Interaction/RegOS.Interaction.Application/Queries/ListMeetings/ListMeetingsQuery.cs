namespace RegOS.Interaction.Application.Queries.ListMeetings;

/// <param name="IncludeConcluded">
/// Held, declined and cancelled meetings are hidden by default: the list
/// answers "what is coming?" A regulatory record is filtered, never deleted.
/// </param>
public sealed record ListMeetingsQuery(bool IncludeConcluded = false);
