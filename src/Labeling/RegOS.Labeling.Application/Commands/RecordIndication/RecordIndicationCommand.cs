using RegOS.Product.Domain.Product;

namespace RegOS.Labeling.Application.Commands.RecordIndication;

/// <param name="ConditionCode">
/// From <c>ClinicalConditionVocabulary</c>. The code is what makes the same
/// authorisation comparable across markets; the text is what the label says.
/// </param>
public sealed record RecordIndicationCommand(
    MedicinalProductId MedicinalProductId,
    string ConditionCode,
    string LabelText,
    DateOnly ApprovedOn);
