using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.Contraindications;

namespace RegOS.Labeling.Application.Commands.AmendContraindicationPopulation;

public sealed record AmendContraindicationPopulationCommand(
    ContraindicationId ContraindicationId,
    PopulationId PopulationId,
    int? AgeLow,
    int? AgeHigh,
    string? AgeUnitCode,
    string GenderCode,
    string? PhysiologicalConditionCode,
    string? Description);
