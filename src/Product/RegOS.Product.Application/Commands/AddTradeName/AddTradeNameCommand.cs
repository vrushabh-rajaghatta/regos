using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.AddTradeName;

/// <param name="Language">
/// An ISO 639-1 code. Parsed into a <see cref="LanguageCode"/> by the handler,
/// so nothing past this boundary holds the raw string.
/// </param>
public sealed record AddTradeNameCommand(
    MedicinalProductId MedicinalProductId,
    string? Language,
    string? Name);
