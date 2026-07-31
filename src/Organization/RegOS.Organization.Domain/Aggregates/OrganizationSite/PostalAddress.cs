using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Organization.Domain.Aggregates.OrganizationSite;

/// <summary>
/// Where a site physically is.
/// </summary>
/// <remarks>
/// <b>Only the country is required</b>, and the value object is deliberately
/// weak because of it. RegOS must not assume it knows everything about a site it
/// did not create: an in-licensed asset routinely arrives as a manufacturer name
/// and a country and nothing more, and refusing to record that would lose the
/// fact entirely. The same principle as
/// <see href="../../../../../docs/adr/ADR-035-submissions-bind-to-a-published-template-version.md">ADR-035</see>
/// — missing upstream data reduces fidelity, it does not block the business.
/// <para>
/// The country is required because it is the only part the model <em>reasons</em>
/// about: it is what the site directory filters by, and a site with no country
/// cannot answer <em>"which manufacturing sites do we have in India?"</em>.
/// Everything else is descriptive.
/// </para>
/// <para>
/// Its job is encapsulation, not enforcement — keeping six descriptive fields
/// off the aggregate's surface, and giving country-aware formatting somewhere to
/// live later.
/// </para>
/// </remarks>
public sealed record PostalAddress
{
    public const int LineMaxLength = 200;

    private PostalAddress()
    {
    }

    public CountryId CountryId { get; private init; }

    public string? Line1 { get; private init; }

    public string? Line2 { get; private init; }

    public string? Line3 { get; private init; }

    public string? City { get; private init; }

    /// <summary>State, province or prefecture — whatever the country calls it.</summary>
    public string? StateProvince { get; private init; }

    public string? PostalCode { get; private init; }

    public static PostalAddress Create(
        CountryId countryId,
        string? line1 = null,
        string? line2 = null,
        string? line3 = null,
        string? city = null,
        string? stateProvince = null,
        string? postalCode = null)
    {
        if (countryId == default)
            throw new DomainException(OrganizationSiteErrors.CountryRequired);

        return new PostalAddress
        {
            CountryId = countryId,
            Line1 = Clean(line1),
            Line2 = Clean(line2),
            Line3 = Clean(line3),
            City = Clean(city),
            StateProvince = Clean(stateProvince),
            PostalCode = Clean(postalCode),
        };
    }

    private static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        return trimmed.Length > LineMaxLength
            ? throw new DomainException(OrganizationSiteErrors.AddressLineTooLong)
            : trimmed;
    }
}
