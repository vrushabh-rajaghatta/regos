using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Labeling.Application.Commands.RecordIndicationDecision;

public sealed record RecordIndicationDecisionCommand(
    IndicationId IndicationId,
    IndicationStatus Status,
    DateOnly OccurredOn,
    string? Note);
