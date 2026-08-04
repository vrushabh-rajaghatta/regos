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

/// <summary>
/// How this pack may be handed over, and how long it keeps.
/// </summary>
/// <remarks>
/// One request over two facts, because one person states both in one sitting.
/// The aggregate keeps them apart — they move on different clocks.
/// </remarks>
/// <param name="ShelfLifeValue">
/// Sent with <paramref name="ShelfLifeUnitCode"/> or not at all, for the reason
/// a pack size is: <em>36</em> alone could be days, months or years.
/// </param>
/// <param name="StorageConditionCodes">
/// Absent or empty means nobody has stated any, which is not the same as
/// <c>NO_SPECIAL_PRECAUTIONS</c> — that one is a conclusion, and it may not be
/// sent beside another condition.
/// </param>
public sealed record StatePackSupplyRequest(
    string? LegalStatusOfSupplyCode,
    decimal? ShelfLifeValue,
    string? ShelfLifeUnitCode,
    string? ShelfLifeText,
    IReadOnlyList<string>? StorageConditionCodes);

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
