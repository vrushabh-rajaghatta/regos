namespace RegOS.Process.Application.Queries.GetProcessObjective;

/// <param name="OccurredOn">The business date — when it became true.</param>
/// <param name="RecordedOnUtc">When RegOS learned. Two clocks, always (ADR-037).</param>
public sealed record ProcessObjectiveHistoryEntry(
    string Status,
    DateOnly OccurredOn,
    DateTime RecordedOnUtc,
    string? Note);
