using RegOS.Product.Domain.Product;

namespace RegOS.Labeling.Application.Queries.GetLabelLanguageCoverage;

/// <summary>
/// <b>"Does this market's labelling cover the languages it is expected in?"</b>
/// — the question EPIC-018 could not ask, because nothing knew the answer's
/// other half.
/// </summary>
/// <remarks>
/// <b>Advisory, and the query's shape says so.</b> It returns what is expected
/// and what is recorded and lets the caller compare; it does not return a
/// verdict, because there is no rule here to be right about. Canada's bilingual
/// obligation falls on the product monograph and on most labels but <em>not</em>
/// on prescription-only, hospital-only or professional-use ones — a distinction
/// that depends on the product and the document, neither of which a country
/// knows (EPIC-022 D4).
/// </remarks>
public sealed record GetLabelLanguageCoverageQuery(
    MedicinalProductId MedicinalProductId);
