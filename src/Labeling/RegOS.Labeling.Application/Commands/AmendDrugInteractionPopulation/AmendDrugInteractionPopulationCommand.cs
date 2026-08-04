using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.DrugInteractions;

namespace RegOS.Labeling.Application.Commands.AmendDrugInteractionPopulation;

public sealed record AmendDrugInteractionPopulationCommand(
    DrugInteractionId DrugInteractionId,
    PopulationId PopulationId,
    int? AgeLow,
    int? AgeHigh,
    string? AgeUnitCode,
    string GenderCode,
    string? PhysiologicalConditionCode,
    string? Description);
