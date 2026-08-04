using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.DrugInteractions;

namespace RegOS.Labeling.Application.Commands.RemoveDrugInteractionPopulation;

public sealed record RemoveDrugInteractionPopulationCommand(
    DrugInteractionId DrugInteractionId,
    PopulationId PopulationId);
