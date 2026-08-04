using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.DrugInteractions;
using RegOS.ReferenceData.Domain.Substances;

namespace RegOS.Labeling.Application.Commands.AddInteractant;

public sealed record AddInteractantCommand(
    DrugInteractionId DrugInteractionId,
    string Description,
    SubstanceId? SubstanceId);
