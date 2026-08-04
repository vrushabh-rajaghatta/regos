using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Labeling.Application.Commands.AmendIndicationPopulation;

public sealed record AmendIndicationPopulationCommand(
    IndicationId IndicationId,
    PopulationId PopulationId,
    int? AgeLow,
    int? AgeHigh,
    string? AgeUnitCode,
    string GenderCode,
    string? PhysiologicalConditionCode,
    string? Description);
