using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.UndesirableEffects;

namespace RegOS.Labeling.Application.Commands.RemoveUndesirableEffectPopulation;

public sealed record RemoveUndesirableEffectPopulationCommand(
    UndesirableEffectId UndesirableEffectId,
    PopulationId PopulationId);
