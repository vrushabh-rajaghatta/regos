using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Persistence.Initialization.ReferenceData;
using RegOS.ReferenceData.Domain.SubmissionType;

using SubmissionTypeEntity =
    RegOS.ReferenceData.Domain.SubmissionType.SubmissionType;

namespace RegOS.Persistence.Initialization.ReferenceData;

internal static class SubmissionTypes
{
    // Authority references use the deterministic ids already seeded in
    // GeographyAndRegulatoryIds — no name lookups, independent of database state.
    public static IReadOnlyList<SubmissionTypeEntity> Data =>
    [
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.Fda510k),
            "FDA_510K",
            "Premarket Notification (510(k))",
            new AuthorityId(GeographyAndRegulatoryIds.FDA)),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.FdaDeNovo),
            "FDA_DENOVO",
            "De Novo Request",
            new AuthorityId(GeographyAndRegulatoryIds.FDA)),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.FdaPma),
            "FDA_PMA",
            "Premarket Approval (PMA)",
            new AuthorityId(GeographyAndRegulatoryIds.FDA)),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.CdscoImport),
            "CDSCO_IMPORT",
            "Import License",
            new AuthorityId(GeographyAndRegulatoryIds.CDSCO)),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.CdscoManufacturing),
            "CDSCO_MANUFACTURING",
            "Manufacturing License",
            new AuthorityId(GeographyAndRegulatoryIds.CDSCO)),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.TgaArtg),
            "TGA_ARTG",
            "ARTG Inclusion",
            new AuthorityId(GeographyAndRegulatoryIds.TGA)),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.HcMdl),
            "HC_MDL",
            "Medical Device Licence",
            new AuthorityId(GeographyAndRegulatoryIds.HealthCanada)),

        // FDA drug (pharma) submission types.
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.FdaInd),
            "FDA_IND",
            "Investigational New Drug Application (IND)",
            new AuthorityId(GeographyAndRegulatoryIds.FDA)),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.FdaNda),
            "FDA_NDA",
            "New Drug Application (NDA)",
            new AuthorityId(GeographyAndRegulatoryIds.FDA))
    ];
}
