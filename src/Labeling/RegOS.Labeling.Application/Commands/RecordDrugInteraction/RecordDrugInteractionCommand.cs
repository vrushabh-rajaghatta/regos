using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Substances;

namespace RegOS.Labeling.Application.Commands.RecordDrugInteraction;

/// <param name="Interactant">
/// What it is with. Required — an interaction with nothing to interact with is
/// not an under-specified statement, it is not one.
/// </param>
/// <param name="InteractantSubstanceId">
/// Optional. Set it and <em>"which of our products interact with warfarin?"</em>
/// becomes a join; leave it null and the text still says what the label says.
/// </param>
public sealed record RecordDrugInteractionCommand(
    MedicinalProductId MedicinalProductId,
    string InteractionTypeCode,
    string LabelText,
    string Interactant,
    SubstanceId? InteractantSubstanceId,
    string? Management,
    string? SeverityCode);
