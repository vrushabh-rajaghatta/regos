using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Labeling.Application.Commands.AddIndicationTherapy;

public sealed record AddIndicationTherapyCommand(
    IndicationId IndicationId,
    string RelationshipCode,
    string Therapy);
