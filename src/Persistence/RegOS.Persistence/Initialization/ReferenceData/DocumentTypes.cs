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
            "Validation Report"),

        // CTD / pharma document types (thin FDA IND slice).
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.CoverLetter),
            "COVER_LETTER",
            "Cover Letter"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.FormFda1571),
            "FDA_1571",
            "Form FDA 1571 (IND Application)"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.InvestigatorsBrochure),
            "IB",
            "Investigator's Brochure"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.NonclinicalOverview),
            "NONCLINICAL_OVERVIEW",
            "Nonclinical Overview (Module 2.4)"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.ClinicalOverview),
            "CLINICAL_OVERVIEW",
            "Clinical Overview (Module 2.5)"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.DrugSubstanceSummary),
            "DRUG_SUBSTANCE",
            "Drug Substance (Module 3.2.S)"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.DrugProductSummary),
            "DRUG_PRODUCT",
            "Drug Product (Module 3.2.P)")
    ];
}
