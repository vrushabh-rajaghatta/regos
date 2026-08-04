using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.UndesirableEffects;

namespace RegOS.Labeling.Application.Commands.AddUndesirableEffectPopulation;

public sealed record AddUndesirableEffectPopulationCommand(
    UndesirableEffectId UndesirableEffectId,
    int? AgeLow,
    int? AgeHigh,
    string? AgeUnitCode,
    string GenderCode,
    string? PhysiologicalConditionCode,
    string? Description);
