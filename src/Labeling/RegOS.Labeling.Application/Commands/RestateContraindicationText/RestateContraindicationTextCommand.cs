using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.Contraindications;

namespace RegOS.Labeling.Application.Commands.RestateContraindicationText;

public sealed record RestateContraindicationTextCommand(
    ContraindicationId ContraindicationId,
    string LabelText);
