using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.UndesirableEffects;

namespace RegOS.Labeling.Application.Commands.AmendUndesirableEffectPopulation;

public sealed record AmendUndesirableEffectPopulationCommand(
    UndesirableEffectId UndesirableEffectId,
    PopulationId PopulationId,
    int? AgeLow,
    int? AgeHigh,
    string? AgeUnitCode,
    string GenderCode,
    string? PhysiologicalConditionCode,
    string? Description);
