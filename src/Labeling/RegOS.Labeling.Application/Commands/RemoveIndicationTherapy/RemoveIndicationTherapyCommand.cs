using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Labeling.Application.Commands.RemoveIndicationTherapy;

public sealed record RemoveIndicationTherapyCommand(
    IndicationId IndicationId,
    OtherTherapyId OtherTherapyId);
