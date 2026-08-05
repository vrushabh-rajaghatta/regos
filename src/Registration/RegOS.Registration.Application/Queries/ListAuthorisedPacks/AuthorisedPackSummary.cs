namespace RegOS.Registration.Application.Queries.ListAuthorisedPacks;

/// <summary>
/// What this market may sell, and what it accepts stability data from.
/// </summary>
/// <remarks>
/// <b>An envelope rather than a column repeated on every row.</b>
/// <paramref name="AcceptsStabilityDataFrom"/> is a fact about the market, and
/// putting it on each pack would say it as many times as there are packs and
/// invite a reader to think it varied between them.
/// </remarks>
/// <param name="AcceptsStabilityDataFrom">
/// The long-term stability conditions this market accepts — Germany
/// <em>25 °C/60% RH</em> or <em>30 °C/65% RH</em>, India <em>30 °C/70% RH</em>.
/// <para>
/// <b>Empty means RegOS holds none for this market</b>, not that the market
/// accepts none — and every pack's
/// <see cref="AuthorisedPackSummary.StabilitySupported"/> is then null, because
/// nothing can be judged against silence.
/// </para>
/// </param>
public sealed record MarketAuthorisedPacks(
    IReadOnlyList<string> AcceptsStabilityDataFrom,
    IReadOnlyList<AuthorisedPackSummary> Packs);

/// <summary>
/// One pack, what authorises it, and how it is supplied.
/// </summary>
/// <remarks>
/// <b>The five stories of EPIC-010b in one row.</b> The pack and its size
/// (S001), how many layers it holds (S002), how it may be supplied and how long
/// it keeps (S003), and which licences authorise it (S005). It is the read that
/// says whether the model was cut in the right places — every fact here comes
/// from a different aggregate and none of them is duplicated.
/// </remarks>
/// <param name="Authorisations">
/// <b>Empty is ordinary, not an error.</b> A pack in design has no licence yet.
/// Several is also ordinary: a partial divestment leaves one pack authorised
/// under two.
/// </param>
/// <param name="LayerCount">
/// How many layers the packaging tree holds. A count rather than the tree
/// itself — this read answers *"is it described?"*, and the tree has its own
/// route for when somebody wants to see it.
/// </param>
/// <param name="StorageConditions">
/// How the pack must be kept — <em>"do not store above 25 °C"</em>. Label
/// instructions, and <b>not</b> <paramref name="TestedAt"/>.
/// </param>
/// <param name="TestedAt">
/// The long-term conditions the shelf life was demonstrated at. Empty means the
/// stability data has not been recorded, which is not a rejection.
/// </param>
/// <param name="StabilitySupported">
/// Whether this market accepts the pack's stability data — the whole of
/// EPIC-022 S004 in one field, derived on every read and never stored (D5).
/// <para>
/// <b>Three-valued, because silence is not a refusal.</b> Null means the
/// question cannot be answered: the pack states no testing condition, or RegOS
/// holds none for this market. <paramref name="TestedAt"/> and the envelope's
/// <c>AcceptsStabilityDataFrom</c> are both present, so a null explains itself
/// without a fourth field — the same trick <c>ExpiryFacts</c> uses.
/// </para>
/// <para>
/// <b>Reported, never enforced.</b> A false here does not stop a pack being
/// recorded, saved or authorised, and no route refuses on it. The EPIC-005
/// expiry precedent: derive the interpretation, and let a person decide.
/// </para>
/// </param>
public sealed record AuthorisedPackSummary(
    Guid PackagedProductId,
    string Description,
    decimal? PackSizeQuantity,
    string? PackSizeUnitDisplay,
    string? PackCode,
    string CurrentMarketingStatus,
    string? LegalStatusOfSupplyDisplay,
    decimal? ShelfLifeValue,
    string? ShelfLifeUnitDisplay,
    string? ShelfLifeText,
    IReadOnlyList<string> StorageConditions,
    IReadOnlyList<string> TestedAt,
    bool? StabilitySupported,
    int LayerCount,
    IReadOnlyList<PackAuthorisationSummary> Authorisations);

/// <param name="AuthorisedOn">
/// The date the pack became authorised under this licence — routinely later
/// than the licence itself, which is why the relationship carries a date rather
/// than being a foreign key (ADR-061 §3).
/// </param>
public sealed record PackAuthorisationSummary(
    Guid PackAuthorisationId,
    Guid RegistrationId,
    string? RegistrationNumber,
    string RegistrationStatus,
    DateOnly AuthorisedOn);
