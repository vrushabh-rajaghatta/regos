using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.AddPack;

/// <param name="PackSizeQuantity">
/// Null with a null unit means the size is not stated yet, which a pack in
/// design genuinely is. Half of one is refused by the aggregate.
/// </param>
/// <param name="StatusDate">
/// The business date the pack came into being — supplied, never the clock, so a
/// migrated portfolio can say when it actually existed.
/// </param>
public sealed record AddPackCommand(
    MedicinalProductId MedicinalProductId,
    string Description,
    decimal? PackSizeQuantity,
    string? PackSizeUnitCode,
    string? PackCode,
    DateOnly StatusDate);
