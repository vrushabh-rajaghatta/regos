using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.AddPresentation;

/// <summary>
/// Records what a product is in one market — the administrable form, how it is
/// given, and what it is counted in.
/// </summary>
/// <param name="DoseFormCode">
/// A code from <c>PharmaceuticalVocabulary.DoseForms</c>, not a display name:
/// the wire carries the code so a re-worded label does not break a caller.
/// </param>
/// <param name="RouteCodes">
/// Several is ordinary — a solution for injection is routinely intravenous
/// <em>and</em> intramuscular. Empty is allowed: a presentation may be recorded
/// before the route is settled.
/// </param>
public sealed record AddPresentationCommand(
    MedicinalProductId MedicinalProductId,
    string Name,
    string? Description,
    string DoseFormCode,
    string? UnitOfPresentationCode,
    IReadOnlyList<string> RouteCodes);
