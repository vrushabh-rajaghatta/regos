using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Persistence.Initialization.ReferenceData;
using RegOS.ReferenceData.Domain.ApplicationType;

using ApplicationTypeEntity =
    RegOS.ReferenceData.Domain.ApplicationType.ApplicationType;

namespace RegOS.Persistence.Initialization.ReferenceData;

internal static class ApplicationTypes
{
    // Authority references use the deterministic ids already seeded in
    // GeographyAndRegulatoryIds — no name lookups, independent of database state.
    public static IReadOnlyList<ApplicationTypeEntity> Data =>
    [
        ApplicationTypeEntity.Create(
            new ApplicationTypeId(ApplicationTypeIds.Fda510k),
            "FDA_510K",
            "Premarket Notification (510(k))",
            new AuthorityId(GeographyAndRegulatoryIds.FDA)),
        ApplicationTypeEntity.Create(
            new ApplicationTypeId(ApplicationTypeIds.FdaDeNovo),
            "FDA_DENOVO",
            "De Novo Request",
            new AuthorityId(GeographyAndRegulatoryIds.FDA)),
        ApplicationTypeEntity.Create(
            new ApplicationTypeId(ApplicationTypeIds.FdaPma),
            "FDA_PMA",
            "Premarket Approval (PMA)",
            new AuthorityId(GeographyAndRegulatoryIds.FDA)),
        ApplicationTypeEntity.Create(
            new ApplicationTypeId(ApplicationTypeIds.CdscoImport),
            "CDSCO_IMPORT",
            "Import License",
            new AuthorityId(GeographyAndRegulatoryIds.CDSCO)),
        ApplicationTypeEntity.Create(
            new ApplicationTypeId(ApplicationTypeIds.CdscoManufacturing),
            "CDSCO_MANUFACTURING",
            "Manufacturing License",
            new AuthorityId(GeographyAndRegulatoryIds.CDSCO)),
        ApplicationTypeEntity.Create(
            new ApplicationTypeId(ApplicationTypeIds.TgaArtg),
            "TGA_ARTG",
            "ARTG Inclusion",
            new AuthorityId(GeographyAndRegulatoryIds.TGA)),
        ApplicationTypeEntity.Create(
            new ApplicationTypeId(ApplicationTypeIds.HcMdl),
            "HC_MDL",
            "Medical Device Licence",
            new AuthorityId(GeographyAndRegulatoryIds.HealthCanada)),

        // FDA drug (pharma) application types.
        //
        // FDA_IND is the only row in this table with a wire token, because it is
        // the only one this project has seen written down — `fdaat4`, read out
        // of FDA's worked examples #21-#24 and recorded in
        // docs/evidence/EPIC-007a/ectd-mapping.md (Level 3).
        //
        // Every other null here means "the token for this row is not in
        // evidence", which is a smaller and more honest claim than "this
        // authority is unmodelled": FDA's own NDA and 510(k) tokens are just as
        // absent as CDSCO's. Rendering fails by name either way.
        ApplicationTypeEntity.Create(
            new ApplicationTypeId(ApplicationTypeIds.FdaInd),
            "FDA_IND",
            "Investigational New Drug Application (IND)",
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            "fdaat4"),
        ApplicationTypeEntity.Create(
            new ApplicationTypeId(ApplicationTypeIds.FdaNda),
            "FDA_NDA",
            "New Drug Application (NDA)",
            new AuthorityId(GeographyAndRegulatoryIds.FDA)),

        // Clinical-trial applications for other authorities (pharma).
        ApplicationTypeEntity.Create(
            new ApplicationTypeId(ApplicationTypeIds.HcCta),
            "HC_CTA",
            "Clinical Trial Application (CTA)",
            new AuthorityId(GeographyAndRegulatoryIds.HealthCanada)),
        ApplicationTypeEntity.Create(
            new ApplicationTypeId(ApplicationTypeIds.TgaCtn),
            "TGA_CTN",
            "Clinical Trial Notification (CTN)",
            new AuthorityId(GeographyAndRegulatoryIds.TGA)),
        ApplicationTypeEntity.Create(
            new ApplicationTypeId(ApplicationTypeIds.CdscoCta),
            "CDSCO_CTA",
            "Clinical Trial Application (Form CT-04)",
            new AuthorityId(GeographyAndRegulatoryIds.CDSCO))
    ];
}
