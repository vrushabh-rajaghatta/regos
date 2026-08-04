using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Labeling.Application.Commands.RestateIndicationText;

public sealed record RestateIndicationTextCommand(
    IndicationId IndicationId,
    string LabelText);
