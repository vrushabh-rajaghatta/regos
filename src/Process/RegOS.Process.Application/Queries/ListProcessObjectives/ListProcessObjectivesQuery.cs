namespace RegOS.Process.Application.Queries.ListProcessObjectives;

/// <param name="IncludeClosed">
/// Achieved and abandoned objectives are hidden by default. They are never
/// deleted (ES-018) — what a company decided not to pursue is as much a part of
/// its record as what it did.
/// </param>
public sealed record ListProcessObjectivesQuery(bool IncludeClosed = false);
