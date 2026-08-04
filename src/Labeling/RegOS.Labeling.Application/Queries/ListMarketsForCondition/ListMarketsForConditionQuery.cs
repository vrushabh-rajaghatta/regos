using RegOS.Product.Domain.Product;

namespace RegOS.Labeling.Application.Queries.ListMarketsForCondition;

/// <summary>
/// <b>"Which markets is this product approved for this condition in?"</b> —
/// EPIC-018's capstone question.
/// </summary>
/// <remarks>
/// <para>
/// <b>Keyed on a condition code, not an indication id, because the question
/// contains a false premise.</b> There is no cross-market "indication X":
/// France's indication and Canada's are separate aggregates with separate
/// wording, populations and decision histories. What they share is the code.
/// </para>
/// <para>
/// <b>Scoped to a global product</b>, because nobody asks "where is diabetes
/// approved?" — they ask "where is <em>this product</em> approved for
/// diabetes?". A tenant-wide answer would conflate two molecules in one list.
/// </para>
/// </remarks>
/// <param name="ConditionCode">
/// The join key across markets, from the vocabulary at
/// <c>/api/indications/vocabulary</c>. A code no market has an indication for
/// is not an error — "nowhere" is a legitimate answer, and demonstrates that
/// the read is driven by the coded condition rather than by whatever happens to
/// have been recorded.
/// </param>
public sealed record ListMarketsForConditionQuery(
    GlobalProductId GlobalProductId,
    string ConditionCode);
