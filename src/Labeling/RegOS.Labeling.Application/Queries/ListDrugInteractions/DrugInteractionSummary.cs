using RegOS.Labeling.Application.Queries.ListIndications;

namespace RegOS.Labeling.Application.Queries.ListDrugInteractions;

/// <param name="SeverityDisplay">
/// Null where the label does not grade it, which is common and is not missing
/// data.
/// </param>
public sealed record DrugInteractionSummary(
    Guid Id,
    string InteractionTypeCode,
    string InteractionTypeDisplay,
    string LabelText,
    string? Management,
    string? SeverityCode,
    string? SeverityDisplay,
    IReadOnlyList<InteractantSummary> Interactants,
    IReadOnlyList<PopulationSummary> Populations);

/// <param name="SubstanceName">
/// Joined from the catalogue when the optional link is set — never stored
/// beside the id, so renaming a substance renames it everywhere at once.
/// </param>
public sealed record InteractantSummary(
    Guid Id,
    string Description,
    Guid? SubstanceId,
    string? SubstanceName);
