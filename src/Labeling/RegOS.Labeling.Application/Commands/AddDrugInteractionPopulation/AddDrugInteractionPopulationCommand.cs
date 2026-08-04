using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.DrugInteractions;

namespace RegOS.Labeling.Application.Commands.AddDrugInteractionPopulation;

public sealed record AddDrugInteractionPopulationCommand(
    DrugInteractionId DrugInteractionId,
    int? AgeLow,
    int? AgeHigh,
    string? AgeUnitCode,
    string GenderCode,
    string? PhysiologicalConditionCode,
    string? Description);
