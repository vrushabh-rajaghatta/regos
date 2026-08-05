namespace RegOS.Api.Endpoints.Presentations;

/// <param name="Role">
/// <c>Active</c> or <c>Excipient</c>. A rule branches on it — an active must
/// declare a strength — so it is a domain word on the wire rather than a code
/// from a vocabulary.
/// </param>
/// <param name="NumeratorUnitCode">
/// A measurement unit — mg, mL, IU. Never a unit of presentation: the
/// presentation already says what article the product comes in.
/// </param>
/// <param name="ManufacturingSourceSiteId">
/// Where this substance comes from. A different stage of the supply chain from
/// the site that makes the finished product (ADR-063 §2), and null means nobody
/// has said rather than that it is unsourced.
/// </param>
public sealed record AddIngredientRequest(
    Guid SubstanceId,
    string Role,
    decimal? NumeratorValue,
    string? NumeratorUnitCode,
    decimal? DenominatorValue,
    string? DenominatorUnitCode,
    Guid? ManufacturingSourceSiteId);

/// <remarks>
/// No substance — a different substance is a different ingredient, so swapping
/// one is add-then-remove.
/// </remarks>
/// <param name="ManufacturingSourceSiteId">
/// Where this substance comes from. A different stage of the supply chain from
/// the site that makes the finished product (ADR-063 §2), and null means nobody
/// has said rather than that it is unsourced.
/// </param>
public sealed record RestateIngredientRequest(
    string Role,
    decimal? NumeratorValue,
    string? NumeratorUnitCode,
    decimal? DenominatorValue,
    string? DenominatorUnitCode,
    Guid? ManufacturingSourceSiteId);

public sealed record AddIngredientResponse(Guid Id);
