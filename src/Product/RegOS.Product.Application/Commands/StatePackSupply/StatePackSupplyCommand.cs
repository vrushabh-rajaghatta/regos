using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.StatePackSupply;

/// <summary>
/// States how a pack may be supplied and how long it keeps.
/// </summary>
/// <remarks>
/// <b>One command over two aggregate methods</b>, and the split is deliberate.
/// Legal status and shelf life are two facts on two clocks — a reclassification
/// is a regulatory decision, a shelf-life extension arrives by variation — so
/// <c>PackagedProduct</c> keeps them apart. But one person states them in one
/// sitting, filling in one section of an SmPC, so the application layer submits
/// them together. The workflow is the use case; the invariants are the domain's.
/// </remarks>
/// <param name="LegalStatusOfSupplyCode">
/// Null withdraws the classification — a pack recorded before its legal status
/// is known has none.
/// </param>
/// <param name="ShelfLifeValue">
/// Sent with <paramref name="ShelfLifeUnitCode"/> or not at all. Kept literal:
/// <em>3 YEAR</em> is stored as three years, never normalised to 36 months.
/// </param>
/// <param name="StorageConditionCodes">
/// Empty means nobody has said. <c>NO_SPECIAL_PRECAUTIONS</c> means somebody
/// checked and none are needed — and it may not sit beside another.
/// </param>
/// <param name="TestedAtCodes">
/// The long-term conditions the shelf life was established at. <b>A different
/// list from <paramref name="StorageConditionCodes"/> drawn from a different
/// vocabulary</b>: those are label instructions, these are study conditions, and
/// a market reads only the second to decide whether the period holds there.
/// Empty means the stability data has not been recorded, which is not a
/// rejection.
/// </param>
public sealed record StatePackSupplyCommand(
    PackagedProductId PackagedProductId,
    string? LegalStatusOfSupplyCode,
    decimal? ShelfLifeValue,
    string? ShelfLifeUnitCode,
    string? ShelfLifeText,
    IReadOnlyList<string> StorageConditionCodes,
    IReadOnlyList<string> TestedAtCodes);
