using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.RecordAtcCode;

/// <param name="AtcCode">
/// Blank clears it. Absence is an ordinary state for a market, so "we do not
/// have this" is a correction rather than a separate act.
/// </param>
public sealed record RecordAtcCodeCommand(
    MedicinalProductId MedicinalProductId,
    string? AtcCode);
