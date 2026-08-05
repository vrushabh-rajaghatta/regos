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
            new CountryId(GeographyAndRegulatoryIds.India)),
        // The two EU member states had no authority at all, so no EU market
        // could hold a registration — found by EPIC-022 S002's browser proof,
        // which could not demonstrate the epic's own headline question without
        // them. The national agencies, not EMA: an Authority hangs off a
        // CountryId, and EMA is the Union's rather than any member state's.
        Authority.Create(
            new AuthorityId(GeographyAndRegulatoryIds.BfArM),
            "BfArM",
            "Bundesinstitut für Arzneimittel und Medizinprodukte",
            new CountryId(GeographyAndRegulatoryIds.Germany)),
        Authority.Create(
            new AuthorityId(GeographyAndRegulatoryIds.ANSM),
            "ANSM",
            "Agence nationale de sécurité du médicament et des produits de santé",
            new CountryId(GeographyAndRegulatoryIds.France))
    ];
}
