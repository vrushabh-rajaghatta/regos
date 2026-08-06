using RegOS.Process.Domain.Aggregates.ProcessPlans;

namespace RegOS.Process.Application.Commands.ChangeProcessPlanStatus;

public sealed record ChangeProcessPlanStatusCommand(
    ProcessPlanId Id,
    ProcessPlanStatus Status,
    DateOnly OccurredOn,
    string? Note = null);
