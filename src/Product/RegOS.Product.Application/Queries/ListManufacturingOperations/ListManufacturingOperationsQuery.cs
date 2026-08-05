using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Queries.ListManufacturingOperations;

/// <summary>
/// <b>"Which sites make this product?"</b> — the question S001 exists for.
/// </summary>
/// <remarks>
/// <b>Keyed on the market, not the global product.</b> Secondary packaging in
/// particular is done per market, and the question this feeds — <em>is this site
/// on <b>this</b> licence?</em> — compares against one market's authorisation
/// (ADR-039, ADR-063).
/// <para>
/// <b>Closed periods are returned too.</b> A site that made this product for
/// four years made it, and hiding the row would make a 2023 filing
/// unexplainable — the same call EPIC-010b made about a pack with no licence.
/// </para>
/// </remarks>
public sealed record ListManufacturingOperationsQuery(
    MedicinalProductId MedicinalProductId);
