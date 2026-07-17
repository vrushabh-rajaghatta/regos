using RegOS.ReferenceData.Domain.DocumentType;

using DocumentTypeEntity =
    RegOS.ReferenceData.Domain.DocumentType.DocumentType;

namespace RegOS.Persistence.Initialization.ReferenceData;

internal static class DocumentTypes
{
    // Platform-provided system types (OrganizationId == null). Deterministic
    // ids from DocumentTypeIds keep seeding independent of database state.
    public static IReadOnlyList<DocumentTypeEntity> Data =>
    [
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.Cer),
            "CER",
            "Clinical Evaluation Report"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.Rmf),
            "RMF",
            "Risk Management File"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.Ssd),
            "SSD",
            "Software Description"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.Ifu),
            "IFU",
            "Instructions for Use"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.Lbl),
            "LBL",
            "Label"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.Rmp),
            "RMP",
            "Risk Management Plan"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.Tvr),
            "TVR",
            "Test Verification Report"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.Val),
            "VAL",
            "Validation Report")
    ];
}
