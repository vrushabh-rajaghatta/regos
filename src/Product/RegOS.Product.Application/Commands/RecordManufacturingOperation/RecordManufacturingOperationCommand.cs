using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.RecordManufacturingOperation;

/// <summary>
/// Records that a site performs an operation for this market's product.
/// </summary>
/// <remarks>
/// <b>Recording, not approving.</b> This says the work happens; whether the
/// licence permits it is a different statement, made by a different aggregate,
/// and the gap between them is the whole point of the epic (EPIC-010c D6).
/// </remarks>
/// <param name="EffectiveFrom">
/// Supplied rather than read from the clock, so an operation recorded today can
/// say it has run since 2019.
/// </param>
public sealed record RecordManufacturingOperationCommand(
    MedicinalProductId MedicinalProductId,
    OrganizationSiteId OrganizationSiteId,
    string OperationCode,
    DateOnly EffectiveFrom);
