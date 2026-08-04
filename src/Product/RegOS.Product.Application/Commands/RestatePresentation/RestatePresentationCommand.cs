using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.RestatePresentation;

/// <summary>
/// Corrects a presentation, as a whole.
/// </summary>
/// <remarks>
/// Every descriptive field, every time — the same five the aggregate's
/// <c>Restate</c> takes. A partial update would offer five ways to leave a
/// presentation half-corrected, and the record is short enough that restating
/// it is what a user is doing anyway.
/// </remarks>
public sealed record RestatePresentationCommand(
    PharmaceuticalProductDetailId PresentationId,
    string Name,
    string? Description,
    string DoseFormCode,
    string? UnitOfPresentationCode,
    IReadOnlyList<string> RouteCodes);
