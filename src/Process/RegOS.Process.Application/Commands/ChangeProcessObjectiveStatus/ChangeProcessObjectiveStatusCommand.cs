using RegOS.Process.Domain.Aggregates.ProcessObjectives;

namespace RegOS.Process.Application.Commands.ChangeProcessObjectiveStatus;

public sealed record ChangeProcessObjectiveStatusCommand(
    ProcessObjectiveId Id,
    ProcessObjectiveStatus Status,
    DateOnly OccurredOn,
    string? Note = null);
