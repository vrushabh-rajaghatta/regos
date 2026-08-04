using RegOS.ReferenceData.Domain.Substances;

namespace RegOS.Product.Application.Queries.ListProductsContainingSubstance;

/// <summary>
/// <em>"Which of our products contain substance X?"</em> — the question
/// EPIC-010a exists to answer.
/// </summary>
/// <remarks>
/// <b>Asked backwards, which is the whole point.</b> An ingredient stores a
/// substance id rather than repeating a name, so this walks
/// <c>Substance → Ingredient → PharmaceuticalProductDetail → MedicinalProduct</c>
/// as joins. A composition that carried names instead could only be read
/// forwards, and this question would be a string match (ADR-058 §1).
/// </remarks>
public sealed record ListProductsContainingSubstanceQuery(SubstanceId SubstanceId);
