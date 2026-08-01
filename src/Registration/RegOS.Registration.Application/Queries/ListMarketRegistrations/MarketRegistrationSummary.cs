namespace RegOS.Registration.Application.Queries.ListMarketRegistrations;

/// <summary>
/// A row in <em>"what do we hold in this market?"</em> — keyed by product,
/// because the country is the question rather than an answer.
/// </summary>
/// <remarks>
/// Deliberately <em>not</em> the same record as
/// <c>ListProductRegistrations.RegistrationSummary</c>. The two are mirror
/// images: one repeats the product in every row, the other the country. A single
/// DTO carrying both would leave every consumer ignoring half its fields, which
/// is coupling rather than reuse.
/// <para>
/// <b>Denormalised on purpose, and across three aggregates.</b> The product
/// name, what it is called there, whether it is on sale, and the licence over it
/// do not belong together in the domain — they belong together in the
/// <em>question</em>, and that is what a read model is for. Writes remain owned;
/// reads compose (ADR-039 principle 7). Nothing here licenses an invariant in
/// the same direction.
/// </para>
/// </remarks>
/// <param name="TradeNames">
/// Every name the product carries in this market, one per language. There is no
/// primary — that would be a new business concept with unanswered questions
/// behind it (who chooses, must exactly one exist, can it differ by authority)
/// and no demonstrated need. If a narrow row shows only one, that is
/// presentation choosing, not the domain.
/// </param>
/// <param name="MarketStatus">
/// Whether the product is on sale here — <b>not</b> the licence's status, and
/// not <paramref name="MarketIsRetired"/>. A product can hold a valid licence
/// and never have launched.
/// </param>
/// <param name="MarketIsRetired">
/// Whether the market <em>record</em> has been excluded from normal work.
/// Surfaced rather than filtered: "what do we hold" is not "what is currently
/// operational", the same call this query already makes by not hiding withdrawn
/// licences.
/// </param>
public sealed record MarketRegistrationSummary(
    Guid RegistrationId,
    Guid MedicinalProductId,
    Guid GlobalProductId,
    string ProductCode,
    string ProductName,
    IReadOnlyList<string> TradeNames,
    string MarketStatus,
    DateOnly? LaunchedOn,
    bool MarketIsRetired,
    Guid AuthorityId,
    string AuthorityName,
    string HolderOrganizationName,
    string? RegistrationNumber,
    string Status,
    DateOnly? ApprovedOn,
    DateOnly? ExpiresOn,
    bool HasRunningValidity,
    int? DaysUntilExpiry,
    bool IsExpired);
