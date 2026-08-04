using RegOS.Labeling.Domain.Aggregates.DrugInteractions;

namespace RegOS.Labeling.Application.Commands.RecordDrugInteraction;

public sealed record RecordDrugInteractionResult(DrugInteractionId Id);
