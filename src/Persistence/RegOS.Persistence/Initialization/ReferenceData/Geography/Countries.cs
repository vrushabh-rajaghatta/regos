using RegOS.ReferenceData.Domain.Geography.Country;

namespace RegOS.Persistence.Initialization.ReferenceData.Geography;

internal static class Countries
{
    public static IReadOnlyList<Country> Data =>
    [
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.UnitedStates),
            "US",
            "United States"),
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.Canada),
            "CA",
            "Canada"),
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.UnitedKingdom),
            "GB",
            "United Kingdom"),
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.Germany),
            "DE",
            "Germany"),
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.France),
            "FR",
            "France"),
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.Japan),
            "JP",
            "Japan"),
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.Australia),
            "AU",
            "Australia"),
        Country.Create(
            new CountryId(GeographyAndRegulatoryIds.India),
            "IN",
            "India")
    ];
}
