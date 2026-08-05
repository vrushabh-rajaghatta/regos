namespace RegOS.Registration.Application.Queries.ListAuthorisedPacks;

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
