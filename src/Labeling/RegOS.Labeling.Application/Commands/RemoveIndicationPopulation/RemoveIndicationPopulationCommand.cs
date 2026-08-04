using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Labeling.Application.Commands.RemoveIndicationPopulation;

public sealed record RemoveIndicationPopulationCommand(
    IndicationId IndicationId,
    PopulationId PopulationId);
