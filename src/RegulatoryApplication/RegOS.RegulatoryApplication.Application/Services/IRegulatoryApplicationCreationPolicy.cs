using RegOS.MasterData.Domain.Geography.Country;
using RegOS.MasterData.Domain.Regulatory.Authority;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;

namespace RegOS.RegulatoryApplication.Application.Services;

/// <summary>
/// Encapsulates the business rules that govern whether a regulatory application
/// may be created. This is platform/business validation (dependencies exist,
/// organization is active, authority belongs to country, no duplicate) — distinct
/// from the aggregate's own invariant validation, which lives in the domain.
///
/// Sibling policies (e.g. submission, withdrawal) will follow the same shape.
/// </summary>
public interface IRegulatoryApplicationCreationPolicy
{
    Task EnsureCanCreateAsync(
        ProductId productId,
        CountryId countryId,
        AuthorityId authorityId,
        OrganizationId organizationId,
        CancellationToken cancellationToken);
}
