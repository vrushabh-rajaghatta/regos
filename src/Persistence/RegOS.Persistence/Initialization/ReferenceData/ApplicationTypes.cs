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
        // FDA device application types, and their nulls are load-bearing.
        //
        // application-type.xml does publish a code for two of these — fdaat10
        // for 510(k), fdaat9 for PMA — and its own comment says they "should
        // only be used in the cross-reference-application-number element"
        // (evidence E32). A cross-reference names some *other* application; it
        // is not this application's type. RegOS has nowhere to record that
        // distinction, so it records no token, and a reader holding FDA's list
        // is not left to conclude these were simply missed.
        //
        // FDA_DENOVO is different again: the list is complete and status-
        // flagged, and there is no De Novo code in it at all. Its null is not
        // "unread" — it is "FDA publishes none", which means a De Novo request
        // has no eCTD application type to be filed under.
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
        // Both tokens here come from FDA's own published list — spec/
        // application-type.xml v1.1, held since 2026-08-03 (evidence E30).
        // `fdaat4` was RegOS's own assertion for a year, flagged in E11 as
        // unevidenced, and turns out to have been right. That does not make a
        // year of asserting it evidence.
        //
        // Every other null here means "the token for this row is not in
        // evidence", which is a smaller and more honest claim than "this
        // authority is unmodelled". Rendering fails by name either way.
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
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            "fdaat1"),

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
