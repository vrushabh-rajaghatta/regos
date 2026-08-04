using RegOS.Product.Domain.Product;

namespace RegOS.Labeling.Application.Commands.RecordUndesirableEffect;

public sealed record RecordUndesirableEffectCommand(
    MedicinalProductId MedicinalProductId,
    string ConditionCode,
    string LabelText,
    string? FrequencyCode);
