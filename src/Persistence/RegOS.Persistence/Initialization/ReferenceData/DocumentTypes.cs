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
            "Drug Product (Module 3.2.P)"),

        // Additional IND artifacts (full FDA IND blueprint).
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.FormFda1572),
            "FDA_1572",
            "Form FDA 1572 (Statement of Investigator)"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.FormFda3674),
            "FDA_3674",
            "Form FDA 3674 (Certification of Compliance, clinicaltrials.gov)"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.StudyProtocol),
            "PROTOCOL",
            "Clinical Study Protocol"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.QualityOverallSummary),
            "QOS",
            "Quality Overall Summary (Module 2.3)"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.NonclinicalSummary),
            "NONCLINICAL_SUMMARY",
            "Nonclinical Written and Tabulated Summaries (Module 2.6)"),
        DocumentTypeEntity.CreateSystemType(
            new DocumentTypeId(DocumentTypeIds.ClinicalSummary),
            "CLINICAL_SUMMARY",
            "Clinical Summary (Module 2.7)")
    ];
}
