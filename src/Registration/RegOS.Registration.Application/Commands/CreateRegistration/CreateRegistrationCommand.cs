using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Registration.Application.Commands.CreateRegistration;

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
    ProductId ProductId,
    CountryId CountryId,
    AuthorityId AuthorityId,
    OrganizationId HolderOrganizationId,
    DateOnly OccurredOn,
    RegulatoryApplicationId? OriginatingApplicationId = null,
    string? Note = null);
