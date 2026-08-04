using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.Contraindications;

namespace RegOS.Labeling.Application.Commands.AddContraindicationPopulation;

public sealed record AddContraindicationPopulationCommand(
    ContraindicationId ContraindicationId,
    int? AgeLow,
    int? AgeHigh,
    string? AgeUnitCode,
    string GenderCode,
    string? PhysiologicalConditionCode,
    string? Description);
