using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.SubmissionType;

namespace RegOS.Persistence.Initialization.ReferenceData.Blueprint;

internal static class RegulatoryTemplates
{
    // Deterministic ids from RegulatoryTemplateIds; the authority and
    // submission-type references reuse the ids already seeded elsewhere.
    public static IReadOnlyList<RegulatoryTemplate> Data =>
    [
        BuildFdaIndCtd()
    ];

    private static RegulatoryTemplate BuildFdaIndCtd()
    {
        var template = RegulatoryTemplate.Create(
            new RegulatoryTemplateId(RegulatoryTemplateIds.FdaIndCtd),
            "FDA_IND_CTD",
            "FDA IND (CTD)",
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            new SubmissionTypeId(SubmissionTypeIds.FdaInd),
            "ICH eCTD / FDA");

        // Build the v1 blueprint on a draft, then publish (freeze) it — a thin
        // CTD slice: sections, the documents they expect, and a couple of
        // validation rules (inert data; the engine that runs them is a later epic).
        var v1 = template.StartDraftVersion();

        var m1 = template.AddSection(
            "M1", "Administrative Information and Prescribing Information", null, 1);
        var m2 = template.AddSection(
            "M2", "Common Technical Document Summaries", null, 2);
        var m3 = template.AddSection("M3", "Quality", null, 3);
        var substance = template.AddSection("3.2.S", "Drug Substance", m3.Id, 1);
        var product = template.AddSection("3.2.P", "Drug Product", m3.Id, 2);
        var m4 = template.AddSection("M4", "Nonclinical Study Reports", null, 4);
        template.AddSection("M5", "Clinical Study Reports", null, 5);

        // The documents each section expects, typed by DocumentType.
        template.AddRequiredDocument(
            m1.Id, new DocumentTypeId(DocumentTypeIds.CoverLetter), true, 1);
        template.AddRequiredDocument(
            m1.Id, new DocumentTypeId(DocumentTypeIds.FormFda1571), true, 2);
        template.AddRequiredDocument(
            m2.Id, new DocumentTypeId(DocumentTypeIds.NonclinicalOverview), true, 1);
        template.AddRequiredDocument(
            m2.Id, new DocumentTypeId(DocumentTypeIds.ClinicalOverview), true, 2);
        template.AddRequiredDocument(
            substance.Id, new DocumentTypeId(DocumentTypeIds.DrugSubstanceSummary), true, 1);
        template.AddRequiredDocument(
            product.Id, new DocumentTypeId(DocumentTypeIds.DrugProductSummary), true, 1);

        // Validation rules the blueprint imposes — checkable constraints beyond
        // structure and content. Data only; nothing executes them yet.
        template.AddValidationRule(
            "FDA-IND-PDF",
            ValidationRuleType.FileFormat,
            ValidationSeverity.Error,
            "All submission documents must be provided as PDF.",
            sectionId: null,
            parameters: "pdf",
            order: 1);
        template.AddValidationRule(
            "FDA-IND-M1-NONEMPTY",
            ValidationRuleType.SectionNotEmpty,
            ValidationSeverity.Error,
            "Module 1 (Administrative Information) must contain at least one document.",
            sectionId: m1.Id,
            parameters: null,
            order: 2);
        template.AddValidationRule(
            "FDA-IND-M4-NONEMPTY",
            ValidationRuleType.SectionNotEmpty,
            ValidationSeverity.Warning,
            "Module 4 (Nonclinical Study Reports) is expected but may be phased.",
            sectionId: m4.Id,
            parameters: null,
            order: 3);

        template.PublishVersion(v1.Id, new DateOnly(2026, 1, 1), DateTime.UtcNow);

        return template;
    }
}
