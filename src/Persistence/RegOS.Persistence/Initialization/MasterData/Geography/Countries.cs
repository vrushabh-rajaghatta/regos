using RegOS.MasterData.Domain.Geography.Country;

namespace RegOS.Persistence.Initialization.MasterData.Geography;

internal static class Countries
{
    public static IReadOnlyList<Country> Data =>
    [
        Country.Create(
            new CountryId(MasterDataIds.UnitedStates),
            "US",
            "United States"),
        Country.Create(
            new CountryId(MasterDataIds.Canada),
            "CA",
            "Canada"),
        Country.Create(
            new CountryId(MasterDataIds.UnitedKingdom),
            "GB",
            "United Kingdom"),
        Country.Create(
            new CountryId(MasterDataIds.Germany),
            "DE",
            "Germany"),
        Country.Create(
            new CountryId(MasterDataIds.France),
            "FR",
            "France"),
        Country.Create(
            new CountryId(MasterDataIds.Japan),
            "JP",
            "Japan"),
        Country.Create(
            new CountryId(MasterDataIds.Australia),
            "AU",
            "Australia"),
        Country.Create(
            new CountryId(MasterDataIds.India),
            "IN",
            "India")
    ];
}
