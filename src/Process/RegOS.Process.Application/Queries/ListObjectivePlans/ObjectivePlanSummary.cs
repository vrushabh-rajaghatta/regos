namespace RegOS.Process.Application.Queries.ListObjectivePlans;

/// <param name="PlannedStartOn">
/// Derived from the plan's steps, never stored — a second copy could disagree
/// with the schedule it summarises.
/// </param>
public sealed record ObjectivePlanSummary(
    Guid Id,
    string Name,
    string Status,
    string DefinitionName,
    int DefinitionVersionNumber,
    bool DefinitionVersionIsSuperseded,
    DateOnly AnchorDate,
    DateOnly? PlannedStartOn,
    DateOnly? PlannedEndOn,
    int StepCount);
