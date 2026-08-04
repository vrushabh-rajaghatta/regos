using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.UndesirableEffects;

namespace RegOS.Labeling.Application.Commands.RestateUndesirableEffectText;

public sealed record RestateUndesirableEffectTextCommand(
    UndesirableEffectId UndesirableEffectId,
    string LabelText);
