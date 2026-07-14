using RegOS.MasterData.Domain.Regulatory.Authority;
using RegOS.Persistence.Initialization.MasterData;
using RegOS.ReferenceData.Domain.SubmissionType;

using SubmissionTypeEntity =
    RegOS.ReferenceData.Domain.SubmissionType.SubmissionType;

namespace RegOS.Persistence.Initialization.ReferenceData;

internal static class SubmissionTypes
{
    // Authority references use the deterministic ids already seeded in
    // MasterDataIds — no name lookups, independent of database state.
    public static IReadOnlyList<SubmissionTypeEntity> Data =>
    [
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.Fda510k),
            "FDA_510K",
            "Premarket Notification (510(k))",
            new AuthorityId(MasterDataIds.FDA)),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.FdaDeNovo),
            "FDA_DENOVO",
            "De Novo Request",
            new AuthorityId(MasterDataIds.FDA)),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.FdaPma),
            "FDA_PMA",
            "Premarket Approval (PMA)",
            new AuthorityId(MasterDataIds.FDA)),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.CdscoImport),
            "CDSCO_IMPORT",
            "Import License",
            new AuthorityId(MasterDataIds.CDSCO)),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.CdscoManufacturing),
            "CDSCO_MANUFACTURING",
            "Manufacturing License",
            new AuthorityId(MasterDataIds.CDSCO)),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.TgaArtg),
            "TGA_ARTG",
            "ARTG Inclusion",
            new AuthorityId(MasterDataIds.TGA)),
        SubmissionTypeEntity.Create(
            new SubmissionTypeId(SubmissionTypeIds.HcMdl),
            "HC_MDL",
            "Medical Device Licence",
            new AuthorityId(MasterDataIds.HealthCanada))
    ];
}
