using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;

using ApplicationTypeEntity = RegOS.ReferenceData.Domain.ApplicationType.ApplicationType;

namespace RegOS.RegulatoryApplication.Application.Services;

/// <summary>
/// Encapsulates the business rules that govern whether a Application
/// may be created. This is platform/business validation (dependencies exist,
/// organization is active, authority belongs to country, no duplicate) — distinct
/// from the aggregate's own invariant validation, which lives in the domain.
///
/// Sibling policies (e.g. submission, withdrawal) will follow the same shape.
/// </summary>
public interface IRegulatoryApplicationCreationPolicy
{
    /// <summary>
    /// Returns the resolved <see cref="ApplicationTypeEntity"/>, because the
    /// aggregate's own invariant — the type must belong to the application's
    /// authority — needs the entity rather than its id, and the policy has
    /// already had to load it to prove it exists. Existence is a policy
    /// question; belonging is the aggregate's.
    /// </summary>
    Task<ApplicationTypeEntity> EnsureCanCreateAsync(
        GlobalProductId globalProductId,
        CountryId countryId,
        AuthorityId authorityId,
        ApplicationTypeId applicationTypeId,
        OrganizationId organizationId,
        CancellationToken cancellationToken);
}
