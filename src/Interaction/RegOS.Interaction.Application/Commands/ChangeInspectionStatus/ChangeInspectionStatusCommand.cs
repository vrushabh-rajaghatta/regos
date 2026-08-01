using RegOS.Interaction.Domain.Inspections;

namespace RegOS.Interaction.Application.Commands.ChangeInspectionStatus;

public sealed record ChangeInspectionStatusCommand(
    InspectionId InspectionId,
    InspectionStatus Target,
    DateOnly OccurredOn,
    string? Note);
