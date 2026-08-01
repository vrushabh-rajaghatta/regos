using RegOS.Interaction.Domain.Inspections;

namespace RegOS.Interaction.Application.Commands.RecordInspectionFindings;

public sealed record RecordInspectionFindingsCommand(
    InspectionId InspectionId,
    string? Findings);
