namespace RegOS.Process.Application.Queries.ListProcessObjectives;

/// <param name="HasMarketRecord">
/// Whether the market-local product that fulfils this objective exists yet. False
/// is the normal state of a proposed objective, not a gap (ADR-065 D8).
/// </param>
public sealed record ProcessObjectiveSummary(
    Guid Id,
    string Name,
    string ProductName,
    string CountryCode,
    string CountryName,
    string Status,
    DateOnly StatedOn,
    DateOnly? TargetCompletionOn,
    DateOnly? AchievedOn,
    bool HasMarketRecord,
    Guid? OwnerUserId);
