using RegOS.Labeling.Domain.Aggregates.LocalLabels;

namespace RegOS.Labeling.Application.Commands.PrintLocalLabelForPack;

/// <summary>
/// Names the pack a local label is printed for, or clears it.
/// </summary>
/// <remarks>
/// <b>The debt EPIC-018 carried, and it named EPIC-010b as the milestone.</b> A
/// carton is printed for a specific pack: the 30 and the 100 are separately
/// approved artworks even when the words on them are identical.
/// <para>
/// <b>Not restricted to artwork.</b> EPIC-018 D2 made artwork a label type
/// rather than an aggregate and recorded the price — the moment a rule reads
/// <c>if (Type == Artwork)</c>, that trade has stopped paying. A container label
/// is printed per pack size anyway, so the branch would be wrong as well as
/// expensive.
/// </para>
/// </remarks>
/// <param name="PackagedProductId">
/// Null clears the link, which is a real act: an artwork reassigned in error is
/// unlinked rather than pointed at the wrong pack.
/// </param>
public sealed record PrintLocalLabelForPackCommand(
    LocalLabelId LocalLabelId,
    Guid? PackagedProductId);
