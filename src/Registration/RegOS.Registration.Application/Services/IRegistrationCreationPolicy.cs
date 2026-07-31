using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Registration.Application.Services;

/// <summary>
/// The cross-context rules a registration must satisfy before it can exist.
/// </summary>
/// <remarks>
/// Deliberately parallel to <c>IRegulatoryApplicationCreationPolicy</c> rather
/// than shared with it. The reference-data checks look the same, but the two
/// policies already disagree on the rule that matters most: an application is
/// unique per (product, country, authority), and a registration explicitly is
/// <em>not</em> — real portfolios hold several authorisations in one market.
/// Extracting a common validator would create a shared dependency between two
/// contexts that are meant to be independent, to save duplicating rules that
/// have already begun to diverge.
/// <para>
/// Two occurrences is not a pattern. If a third context needs these checks,
/// that is the point to extract — not before.
/// </para>
/// </remarks>
public interface IRegistrationCreationPolicy
{
    Task EnsureCanCreateAsync(
        ProductId productId,
        CountryId countryId,
        AuthorityId authorityId,
        OrganizationId holderOrganizationId,
        RegulatoryApplicationId? originatingApplicationId,
        CancellationToken cancellationToken);
}
