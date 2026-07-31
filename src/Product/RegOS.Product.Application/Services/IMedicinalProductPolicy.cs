using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;

namespace RegOS.Product.Application.Services;

/// <summary>
/// The cross-aggregate rules a market-local product must satisfy before it can
/// exist: the global product it localises and the country it is marketed in
/// must both be real.
/// </summary>
/// <remarks>
/// Deliberately short. The rule a reader will look for and not find is
/// uniqueness on (global product, country) — see
/// <see cref="MedicinalProduct"/> for why there isn't one.
/// </remarks>
public interface IMedicinalProductPolicy
{
    Task EnsureCanCreateAsync(
        GlobalProductId globalProductId,
        CountryId countryId,
        CancellationToken cancellationToken);
}
