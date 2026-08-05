namespace RegOS.Api.Endpoints.Manufacturing;

/// <summary>
/// Records that a site performs an operation for this market's product.
/// </summary>
/// <param name="OperationCode">
/// From the manufacturing vocabulary — API manufacture, finished product,
/// primary or secondary packaging, QC testing, batch release, importation.
/// </param>
/// <param name="EffectiveFrom">
/// Supplied rather than read from the clock, so an operation recorded today can
/// say it has run since 2019.
/// </param>
public sealed record RecordManufacturingOperationRequest(
    Guid OrganizationSiteId,
    string OperationCode,
    DateOnly EffectiveFrom);

/// <remarks>
/// Its own route rather than a field on the record above: recording that work
/// happens and recording that it stopped are two acts, months or years apart.
/// </remarks>
public sealed record CeaseManufacturingOperationRequest(DateOnly CeasedOn);

public sealed record ManufacturingOperationResponse(Guid Id);
