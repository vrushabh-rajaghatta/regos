using RegOS.MasterData.Domain.Geography.Country;
using RegOS.MasterData.Domain.Regulatory.Authority;

namespace RegOS.Persistence.Initialization.MasterData.Regulatory;

internal static class Authorities
{
    public static IReadOnlyList<Authority> Data =>
    [
        Authority.Create(
            new AuthorityId(MasterDataIds.FDA),
            "FDA",
            "Food and Drug Administration",
            new CountryId(MasterDataIds.UnitedStates)),
        Authority.Create(
            new AuthorityId(MasterDataIds.HealthCanada),
            "HC",
            "Health Canada",
            new CountryId(MasterDataIds.Canada)),
        Authority.Create(
            new AuthorityId(MasterDataIds.MHRA),
            "MHRA",
            "Medicines and Healthcare products Regulatory Agency",
            new CountryId(MasterDataIds.UnitedKingdom)),
        Authority.Create(
            new AuthorityId(MasterDataIds.PMDA),
            "PMDA",
            "Pharmaceuticals and Medical Devices Agency",
            new CountryId(MasterDataIds.Japan)),
        Authority.Create(
            new AuthorityId(MasterDataIds.TGA),
            "TGA",
            "Therapeutic Goods Administration",
            new CountryId(MasterDataIds.Australia)),
        Authority.Create(
            new AuthorityId(MasterDataIds.CDSCO),
            "CDSCO",
            "Central Drugs Standard Control Organisation",
            new CountryId(MasterDataIds.India))
    ];
}
