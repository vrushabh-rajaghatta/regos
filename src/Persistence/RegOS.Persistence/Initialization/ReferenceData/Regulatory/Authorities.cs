using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;

namespace RegOS.Persistence.Initialization.ReferenceData.Regulatory;

internal static class Authorities
{
    public static IReadOnlyList<Authority> Data =>
    [
        Authority.Create(
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            "FDA",
            "Food and Drug Administration",
            new CountryId(GeographyAndRegulatoryIds.UnitedStates)),
        Authority.Create(
            new AuthorityId(GeographyAndRegulatoryIds.HealthCanada),
            "HC",
            "Health Canada",
            new CountryId(GeographyAndRegulatoryIds.Canada)),
        Authority.Create(
            new AuthorityId(GeographyAndRegulatoryIds.MHRA),
            "MHRA",
            "Medicines and Healthcare products Regulatory Agency",
            new CountryId(GeographyAndRegulatoryIds.UnitedKingdom)),
        Authority.Create(
            new AuthorityId(GeographyAndRegulatoryIds.PMDA),
            "PMDA",
            "Pharmaceuticals and Medical Devices Agency",
            new CountryId(GeographyAndRegulatoryIds.Japan)),
        Authority.Create(
            new AuthorityId(GeographyAndRegulatoryIds.TGA),
            "TGA",
            "Therapeutic Goods Administration",
            new CountryId(GeographyAndRegulatoryIds.Australia)),
        Authority.Create(
            new AuthorityId(GeographyAndRegulatoryIds.CDSCO),
            "CDSCO",
            "Central Drugs Standard Control Organisation",
            new CountryId(GeographyAndRegulatoryIds.India))
    ];
}
