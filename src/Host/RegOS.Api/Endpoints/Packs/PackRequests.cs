using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.Packs;

/// <param name="PackSizeQuantity">
/// Sent with <paramref name="PackSizeUnitCode"/> or not at all — the aggregate
/// refuses half a pack size, because <em>30</em> alone could be tablets,
/// millilitres or vials.
/// </param>
public sealed record PackRequest(
    string Description,
    decimal? PackSizeQuantity,
    string? PackSizeUnitCode,
    string? PackCode,
    DateOnly StatusDate);

/// <remarks>
/// No <c>StatusDate</c>: restating what a pack <em>is</em> does not move its
/// commercial history, which has its own route.
/// </remarks>
public sealed record RestatePackRequest(
    string Description,
    decimal? PackSizeQuantity,
    string? PackSizeUnitCode,
    string? PackCode);

/// <remarks>
/// The enum on the wire, matching <c>ChangeMarketStatusRequest</c> one tier up:
/// an unrecognised word is refused by model binding rather than by a string
/// comparison the domain would have to repeat.
/// </remarks>
public sealed record ChangePackMarketingStatusRequest(
    PackageMarketingStatus Status,
    DateOnly OccurredOn,
    string? Note);

public sealed record PackResponse(Guid Id);
