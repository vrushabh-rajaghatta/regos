namespace RegOS.Labeling.Application.Queries.ListMarketsForCondition;

/// <summary>
/// One market's standing on one condition.
/// </summary>
/// <remarks>
/// <b>Every market that has an indication for the condition is returned, each
/// with its current standing</b> — not only the approved ones. <em>Japan
/// Approved · France Withdrawn · Canada Approved</em> answers both directions of
/// the same question; a silently filtered list would need a second endpoint to
/// answer "and where did we lose it?".
/// </remarks>
/// <param name="LabelText">
/// <b>What this market's label actually says</b>, beside the code every market
/// shares. The pair is ADR-059's principle in one row: a coded regulatory fact,
/// published in each market's own words. It is shown, never compared — cross-market
/// divergence reporting is EPIC-011.
/// </param>
/// <param name="Since">
/// When the standing in <paramref name="Status"/> took effect, in business time.
/// </param>
/// <param name="IsInForce">
/// Whether the authorisation still stands. <c>Status</c> is what was decided;
/// this is whether it survived. Derived rather than stored, and the enum's own
/// documentation is the rule: <em>Restricted</em> is "narrowed, and still
/// authorised", <em>Withdrawn</em> is "no longer authorised". Three of the four
/// statuses are approvals.
/// </param>
public sealed record ConditionMarketSummary(
    Guid MedicinalProductId,
    Guid CountryId,
    string CountryName,
    string CountryCode,
    Guid IndicationId,
    string LabelText,
    string Status,
    DateOnly Since,
    bool IsInForce);
