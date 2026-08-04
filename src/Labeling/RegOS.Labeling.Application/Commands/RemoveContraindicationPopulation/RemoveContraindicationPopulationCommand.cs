using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.Contraindications;

namespace RegOS.Labeling.Application.Commands.RemoveContraindicationPopulation;

public sealed record RemoveContraindicationPopulationCommand(
    ContraindicationId ContraindicationId,
    PopulationId PopulationId);
