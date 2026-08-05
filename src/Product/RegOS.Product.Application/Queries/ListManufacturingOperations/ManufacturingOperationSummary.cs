namespace RegOS.Product.Application.Queries.ListManufacturingOperations;

/// <summary>
/// One site, what it does for this market, and over what period.
/// </summary>
/// <param name="SiteName">
/// Read from the site rather than copied onto the operation. There is
/// deliberately no <c>ManufacturerName</c> stored anywhere (ADR-063 §3) — a
/// copied name is a second place for the truth to live and the first to go
/// stale when a plant is renamed.
/// </param>
/// <param name="SiteIdentifiers">
/// What registries know the site as — an FEI, a DUNS. What a filing quotes, and
/// the reason this read joins the site at all.
/// </param>
/// <param name="CeasedOn">
/// Null while the site still performs the operation. A closed period is history,
/// not a deletion.
/// </param>
public sealed record ManufacturingOperationSummary(
    Guid ManufacturingOperationId,
    Guid OrganizationSiteId,
    string SiteName,
    string SiteCountryName,
    string SiteTypeName,
    IReadOnlyList<string> SiteIdentifiers,
    string OperationCode,
    string OperationDisplay,
    DateOnly EffectiveFrom,
    DateOnly? CeasedOn,
    bool IsCurrent);
