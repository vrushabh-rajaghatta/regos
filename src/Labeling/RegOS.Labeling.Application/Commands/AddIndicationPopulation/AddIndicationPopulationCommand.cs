using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Labeling.Application.Commands.AddIndicationPopulation;

public sealed record AddIndicationPopulationCommand(
    IndicationId IndicationId,
    int? AgeLow,
    int? AgeHigh,
    string? AgeUnitCode,
    string GenderCode,
    string? PhysiologicalConditionCode,
    string? Description);
