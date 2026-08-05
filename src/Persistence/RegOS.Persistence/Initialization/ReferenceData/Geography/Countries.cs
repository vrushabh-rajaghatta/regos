using RegOS.ReferenceData.Domain.Geography.Country;

namespace RegOS.Persistence.Initialization.ReferenceData.Geography;

/// <summary>
/// The eight jurisdictions RegOS demonstrates against.
/// </summary>
/// <remarks>
/// <b>Demonstration seed data only. These records intentionally do not
/// represent an authoritative geography register.</b> The same statement
/// <c>Substance</c> carries, and for the same reason: eight hand-verified rows
/// and a licensed register are different evidence levels, and only one of them
/// can be widened without going and fetching something
/// (<see href="../../../../../docs/evidence/EPIC-022/iso-3166-1.md">E36</see>).
/// <para>
/// <b>The alpha-3 code and the ISO name are not derivable from the alpha-2 code
/// or the common name.</b> <em>GB</em> is <em>GBR</em>, and <em>"United
/// Kingdom"</em> is <em>"United Kingdom of Great Britain and Northern
/// Ireland"</em> — every value below was read off the register rather than
/// inferred.
/// </para>
/// </remarks>
internal static class Countries
{
    public static IReadOnlyList<Country> Data =>
    [
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.UnitedStates),
            "US",
            "USA",
            "United States",
            "United States of America"),
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.Canada),
            "CA",
            "CAN",
            "Canada",
            "Canada"),
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.UnitedKingdom),
            "GB",
            "GBR",
            "United Kingdom",
            "United Kingdom of Great Britain and Northern Ireland"),
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.Germany),
            "DE",
            "DEU",
            "Germany",
            "Germany"),
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.France),
            "FR",
            "FRA",
            "France",
            "France"),
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.Japan),
            "JP",
            "JPN",
            "Japan",
            "Japan"),
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.Australia),
            "AU",
            "AUS",
            "Australia",
            "Australia"),
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.India),
            "IN",
            "IND",
            "India",
            "India")
    ];
}
