using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.ChangePackMarketingStatus;

/// <param name="OccurredOn">
/// The business date this became true for the pack — never the clock, and never
/// earlier than the status it replaces.
/// </param>
public sealed record ChangePackMarketingStatusCommand(
    PackagedProductId PackagedProductId,
    PackageMarketingStatus Status,
    DateOnly OccurredOn,
    string? Note);
