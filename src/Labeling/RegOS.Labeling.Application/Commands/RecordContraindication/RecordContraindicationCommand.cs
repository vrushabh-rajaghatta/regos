using RegOS.Product.Domain.Product;

namespace RegOS.Labeling.Application.Commands.RecordContraindication;

public sealed record RecordContraindicationCommand(
    MedicinalProductId MedicinalProductId,
    string ConditionCode,
    string LabelText);
