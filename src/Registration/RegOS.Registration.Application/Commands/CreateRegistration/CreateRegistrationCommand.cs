using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Registration.Application.Commands.CreateRegistration;

/// <param name="MedicinalProductId">
/// The market-local product being authorised — named explicitly, never resolved
/// from a (product, country) pair. Several medicinal products may exist for one
/// pair, so resolving would mean choosing a business object on the user's
/// behalf, non-deterministically. Pick-or-create is the client's job.
/// </param>
/// <param name="OccurredOn">
/// The business date this registration entered its first status. Supplied by
/// the caller, never taken from the clock, so a portfolio migrated from a
/// legacy register can state when things actually happened.
/// </param>
/// <param name="OriginatingApplicationId">
/// Optional: acquired and in-licensed products carry authorisations RegOS never
/// witnessed being filed.
/// </param>
public sealed record CreateRegistrationCommand(
    MedicinalProductId MedicinalProductId,
    AuthorityId AuthorityId,
    OrganizationId HolderOrganizationId,
    DateOnly OccurredOn,
    RegulatoryApplicationId? OriginatingApplicationId = null,
    string? Note = null);
