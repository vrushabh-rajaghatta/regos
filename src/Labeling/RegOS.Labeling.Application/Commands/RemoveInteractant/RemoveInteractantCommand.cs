using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.DrugInteractions;

namespace RegOS.Labeling.Application.Commands.RemoveInteractant;

public sealed record RemoveInteractantCommand(
    DrugInteractionId DrugInteractionId,
    InteractantId InteractantId);
